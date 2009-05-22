using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Math.Primitives;
using System.Diagnostics;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.Simulation
{
    public class PlayerControllerProperty : Property
    {
        #region flags
        private static readonly bool RightStickFlame = false;
        private static readonly bool LeftStickSelection = true;
        public static readonly bool ImuneToIslandPush = true;
        #endregion

        #region button assignments
        // eps for registering move
        private static readonly float StickMovementEps = 0.1f;
        
        // gamepad buttons
        private static readonly Buttons[] RepulsionButtons = { Buttons.LeftTrigger };
        private static readonly Buttons[] JetpackButtons = { Buttons.A };
        private static readonly Buttons[] IceSpikeButtons = { Buttons.X };
        private static readonly Buttons[] FlamethrowerButtons = { Buttons.Y };
        private static readonly Buttons[] HitButtons = { Buttons.B };
        private static readonly Buttons[] RunButtons = { Buttons.RightTrigger };

        // keyboard keys
        private static readonly Keys JetpackKey = Keys.Space;
        private static readonly Keys IceSpikeKey = Keys.Q;
        private static readonly Keys HitKey = Keys.E;
        private static readonly Keys FlamethrowerKey = Keys.R;
        private static readonly Keys RunKey = Keys.LeftControl;
        #endregion


        private Entity player;
        private Entity constants;
        private LevelData templates;

        private float landedAt = -1;
        private bool won = false;

        private Entity activeIsland = null;

        private readonly Random rand = new Random(DateTime.Now.Millisecond);
        private double respawnStartedAt = 0;
        private Entity deathExplosion = null;

        private bool jetpackActive = false;

        private int iceSpikeCount = 0;
        private int iceSpikeRemovedCount = 0;
        private float iceSpikeFiredAt = 0;

        private double hitPerformedAt = 0;

        private float islandSelectedAt = 0;
        private Entity selectedIsland = null;
        private bool selectedIslandInFreeJumpRange = false;
        private Vector3 islandSelectionLastStickDir = Vector3.Zero;
        private Entity destinationIsland = null;
        private float destinationOrigDist = 0;
        private float destinationOrigY = 0;
        private Vector3 lastIslandDir = Vector3.Zero;
        private float islandJumpPerformedAt = 0;

        private Entity simpleJumpIsland = null;

        private bool repulsionActive = false;
        private Vector3 islandRepulsionLastStickDir = Vector3.Zero;

        Entity flame = null;
        Entity arrow;

        private SoundEffect jetpackSound;
        private SoundEffectInstance jetpackSoundInstance;
        private SoundEffect flameThrowerSound;
        private SoundEffectInstance flameThrowerSoundInstance;

        // values which get reset on each update
        private float collisionAt = float.MinValue;
        private float movedAt = float.MinValue;
        Vector3 previousPosition;

        public PlayerControllerProperty()
        {
        }

        public void OnAttached(AbstractEntity player)
        {
            (player as Entity).Update += OnUpdate;

            this.player = player as Entity;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.templates = Game.Instance.ContentManager.Load<LevelData>("Level/Common/DynamicTemplates");

            player.AddQuaternionAttribute("rotation", Quaternion.Identity);
            player.AddVector3Attribute("velocity", Vector3.Zero);

            player.AddVector3Attribute("collision_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("player_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddVector3Attribute("island_jump_velocity", Vector3.Zero);

            player.AddIntAttribute("energy", constants.GetInt("max_energy"));
            player.AddIntAttribute("health", constants.GetInt("max_health"));
            player.GetAttribute<IntAttribute>("health").ValueChanged += HealthChangeHandler;

            player.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            player.AddIntAttribute("jumps", 0);
            player.AddFloatAttribute("repulsion_seconds", 0);

            player.AddIntAttribute("kills", 0);
            player.AddIntAttribute("deaths", 0);

            player.AddIntAttribute("frozen", 0);
            player.AddStringAttribute("active_island", "");
            player.AddStringAttribute("jump_island", "");

            Game.Instance.Simulation.EntityManager.EntityRemoved += EntityRemovedHandler;
            player.GetProperty<CollisionProperty>("collision").OnContact += PlayerCollisionHandler;

            jetpackSound = Game.Instance.ContentManager.Load<SoundEffect>("Sounds/jetpack");
            flameThrowerSound = Game.Instance.ContentManager.Load<SoundEffect>("Sounds/flamethrower");

            arrow = new Entity("arrow" + "_" + player.Name);
            arrow.AddStringAttribute("player", player.Name);

            arrow.AddVector3Attribute("color1", player.GetVector3("color1"));
            arrow.AddVector3Attribute("color2", player.GetVector3("color2"));

            Game.Instance.Simulation.EntityManager.AddDeferred(arrow, "arrow_base", templates);

            PositionOnRandomIsland();

            this.previousPosition = player.GetVector3("position");
        }

        public void OnDetached(AbstractEntity player)
        {
            (player as Entity).Update -= OnUpdate;

            if (arrow != null && Game.Instance.Simulation.EntityManager.ContainsEntity(arrow))
            {
                Game.Instance.Simulation.EntityManager.Remove(arrow);
            }

            if (flame != null && Game.Instance.Simulation.EntityManager.ContainsEntity(flame))
            {
                Game.Instance.Simulation.EntityManager.Remove(flame);
            }

            Game.Instance.Simulation.EntityManager.EntityRemoved -= EntityRemovedHandler;

            player.GetAttribute<IntAttribute>("health").ValueChanged -= HealthChangeHandler;

            if (player.HasProperty("collision"))
            {
                player.GetProperty<CollisionProperty>("collision").OnContact -= PlayerCollisionHandler;
            }
        }

        private void OnUpdate(Entity player, SimulationTime simTime)
        {
            if (Game.Instance.Simulation.Phase == SimulationPhase.Intro)
            {
                PerformIntroMovement(player, simTime);
                return;
            }

            float dt = simTime.Dt;
            float at = simTime.At;

            if (CheckAndPerformDeath(player, at))
            {
                // don't execute any other code as long as player is dead
                return;
            }

            #region collision reaction
            // reset collision response
            if (simTime.At > collisionAt + 250) // todo: extract constant
            {
                player.SetVector3("collision_pushback_velocity", Vector3.Zero);
            }
            #endregion

            PlayerIndex playerIndex = (PlayerIndex)player.GetInt("game_pad_index");
            Vector3 playerPosition = player.GetVector3("position");
            Vector3 playerVelocity = player.GetVector3("velocity");
            Vector3 collisionPushbackVelocity = player.GetVector3("collision_pushback_velocity");
            Vector3 playerPushbackVelocity = player.GetVector3("player_pushback_velocity");
            Vector3 hitPushbackVelocity = player.GetVector3("hit_pushback_velocity");

            int fuel = player.GetInt("fuel");

            // reset some stuff
            previousPosition = playerPosition;

            // get input
            if (Game.Instance.Simulation.Phase == SimulationPhase.Game)
            {
                controllerInput.Update(playerIndex, simTime);
            }
            else
            {
                // don't take any new input events
                controllerInput.Reset();
            }

            #region movement

            // jetpack
            PerformJetpackMovement(simTime, dt, ref playerVelocity, ref fuel);

            // xz movement
            PerformStickMovement(player, dt, ref playerPosition, fuel);

            // perform island jump
            PerformIslandJump(player, dt, at, ref playerPosition, ref playerVelocity);

            // perform simple jump on self
            PerformSimpleJump(ref playerPosition);

            // only apply velocity if not on island
            ApplyGravity(dt, ref playerPosition, ref playerVelocity);

            // apply current velocity
            playerPosition += playerVelocity * dt;

//            Console.WriteLine();
//            Console.WriteLine("at: " + (int)gameTime.TotalGameTime.TotalMilliseconds);
//            Console.WriteLine("velocity: " + playerVelocity + " led to change from " + previousPosition + " to " + playerPosition);

            // pushback
            Simulation.ApplyPushback(ref playerPosition, ref collisionPushbackVelocity, 0f /*constants.GetFloat("player_pushback_deacceleration")*/);
            Simulation.ApplyPushback(ref playerPosition, ref playerPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));
            Simulation.ApplyPushback(ref playerPosition, ref hitPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));

            // frozen!?
            PerformFrozenSlowdown(player, simTime, ref playerPosition);

            #endregion

            #region actions

            if (Game.Instance.Simulation.Phase == SimulationPhase.Game)
            {
                PerformIceSpikeAction(player, at, playerPosition);
                PerformFlamethrowerAction(player, ref playerPosition);
                PerformIslandRepulsionAction(simTime, ref fuel);
                PerformIslandSelectionAction(at, ref playerPosition);
                PerformIslandJumpAction(ref playerPosition, ref playerVelocity);
            }
            #endregion

            #region recharge
            // recharge energy
            if (flame == null)
            {
                Game.Instance.Simulation.ApplyPerSecondAddition(player, "energy_recharge", constants.GetInt("energy_recharge_per_second"),
                    player.GetIntAttribute("energy"));
            }

            // recharge fuel
            if (!controllerInput.jetpackButtonHold)
            {
                if (activeIsland == null)
                {
                    fuel += (int)(simTime.DtMs * constants.GetFloat("fuel_recharge_multiplier"));
                }
                else
                {
                    // faster recharge standing on island, but only if jetpack was not used for repulsion
                    fuel += (int)(simTime.DtMs * constants.GetFloat("fuel_recharge_multiplier_island"));
                }
            }
            #endregion

            // update player attributes
            player.SetInt("fuel", fuel);

            // player.SetVector3("position", new Vector3(0, 300, 200));

            player.SetVector3("position", playerPosition);
            player.SetVector3("velocity", playerVelocity);
            player.SetVector3("collision_pushback_velocity", collisionPushbackVelocity);
            player.SetVector3("player_pushback_velocity", playerPushbackVelocity);
            player.SetVector3("hit_pushback_velocity", hitPushbackVelocity);

            CheckPlayerAttributeRanges(player);

            // reset stuff
            collisionAt = float.MinValue;

            // check collision with lava
            Entity lava = Game.Instance.Simulation.EntityManager["lava"];
            if (playerPosition.Y <= 0) // todo: extract constant?
                PlayerLavaCollisionHandler(simTime, player, lava);

            if(controllerInput.StartHitAnimation)
            {
                controllerInput.StartHitAnimation = false;
                player.GetProperty<RobotRenderProperty>("render").NextOnceState = "hit";
            }

            Debug.Assert(!(selectedIsland == null) || !arrow.HasProperty("render"));
        }

        private void PerformIntroMovement(Entity player, SimulationTime simTime)
        {
            if (landedAt > 0)
            {
                if (simTime.At > landedAt + 1000) // extract constant
                {
                    player.SetBool("ready", true);
                }
                return;
            }

            Vector3 velocity = -Vector3.UnitY * constants.GetFloat("max_gravity_speed");
            Vector3 position = player.GetVector3("position") + velocity * simTime.Dt;

            Vector3 surfacePos;
            if (Simulation.GetPositionOnSurface(ref position, activeIsland, out surfacePos))
            {
                if (position.Y < surfacePos.Y)
                {
                    position = surfacePos;

                    player.GetProperty<RobotRenderProperty>("render").NextOnceState = "jump_end";
                    activeIsland.GetProperty<IslandRenderProperty>("render").Squash();

                    landedAt = simTime.At;
                }
            }
            else
            {
                throw new Exception("island's gone :(");
            }

            player.SetVector3("position", position);
        }

        private void PerformSimpleJump(ref Vector3 playerPosition)
        {
            if (simpleJumpIsland != null)
            {
                // check position a bit further in walking direction to be still on island
                float islandNonWalkingRangeMultiplier = constants.GetFloat("island_non_walking_range_multiplier");
                Vector3 checkPos = playerPosition + new Vector3(controllerInput.leftStickX * islandNonWalkingRangeMultiplier,
                    0, controllerInput.leftStickY * islandNonWalkingRangeMultiplier); // todo: extract constant

                Vector3 isectPt;
                if (!Simulation.GetPositionOnSurface(ref checkPos, simpleJumpIsland, out isectPt))
                {
                    // prohibit movement
                    playerPosition.X = previousPosition.X;
                    playerPosition.Z = previousPosition.Z;
                }
            }
        }

        private void PerformIslandJumpAction(ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            if (controllerInput.jetpackButtonPressed
                && !repulsionActive
                && activeIsland != null
                && player.GetInt("frozen") <= 0)
            {
                // island jump start
                if (selectedIsland != null
                    && (player.GetInt("jumps") > 0 // far ranged jumps avail?
                    || selectedIslandInFreeJumpRange) // or only near jump?
                )
                {
                    if (!selectedIslandInFreeJumpRange)
                        player.SetInt("jumps", player.GetInt("jumps") - 1);
                    StartIslandJump(selectedIsland, ref playerPosition, ref playerVelocity);
                }
                else // only jump up
                {
                    StartSimpleJump(ref playerVelocity);
                }
            }
        }

        private void StartSimpleJump(ref Vector3 playerVelocity)
        {
            simpleJumpIsland = activeIsland;

            LeaveActiveIsland();

            // set attribute
            player.SetString("jump_island", simpleJumpIsland.Name);

            // ensure we trak island movement
            simpleJumpIsland.GetAttribute<Vector3Attribute>("position").ValueChanged += IslandPositionHandler;

            // initiate jump
            playerVelocity = (float)Math.Sqrt(constants.GetFloat("simple_jump_height") / constants.GetVector3("gravity_acceleration").Length())
                * -constants.GetVector3("simple_jump_gravity_acceleration");

            // and adapt model
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "jump";
        }

        private void StartIslandJump(Entity island, ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            destinationIsland = island;

            LeaveActiveIsland();

            // calculate time to travel to island (in xz plane) using an iterative approach
            Vector3 islandDir = GetLandingPosition(destinationIsland) - playerPosition;
            islandDir.Y = 0;
            lastIslandDir = islandDir;
            destinationOrigDist = islandDir.Length();
            destinationOrigY = playerPosition.Y;

            island.GetAttribute<Vector3Attribute>("position").ValueChanged += IslandPositionHandler;

            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "jump";
        }

        private void PerformIslandSelectionAction(float at, ref Vector3 playerPosition)
        {
            if (activeIsland != null && player.GetInt("frozen") <= 0)
            {
                if (/*controllerInput.rightStickMoved
                    &&*/
                         activeIsland != null) // must be standing on island
                {
                    //    Vector3 selectionDirection = new Vector3(controllerInput.rightStickX, 0, controllerInput.rightStickY);
                    //  stickDir.Normalize();
                    Vector3 selectionDirection = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                    // only allow reselection if stick moved slightly
                    bool stickMoved = Vector3.Dot(islandSelectionLastStickDir, selectionDirection) < constants.GetFloat("island_reselection_max_value");
                    if (/*(selectedIsland == null || stickMoved)
                        && */
                              at > islandSelectedAt + constants.GetFloat("island_reselection_timeout"))
                    {
                        //                        Console.WriteLine("new selection (old: " + ((selectedIsland != null) ? selectedIsland.Name : "") + "): " + lastStickDir + "." + stickDir + " = " +
                        //                            Vector3.Dot(lastStickDir, stickDir));

                        // select closest island in direction of stick
                        islandSelectionLastStickDir = selectionDirection;
                        Entity bestIsland = SelectBestIslandInDirection(ref selectionDirection);

                        // new island selected
                        if (bestIsland != null)
                        {
                            selectedIsland = bestIsland;
                            islandSelectedAt = at;
                            arrow.SetString("island", selectedIsland.Name);

                            if (!arrow.HasProperty("render"))
                            {
                                arrow.AddProperty("render", new ArrowRenderProperty());
                                arrow.AddProperty("shadow_cast", new ShadowCastProperty());
                            }

                            // reset in range indicator
                            //selectedIslandInFreeJumpRange = false;
                            // hacky hack hack
                            selectedIslandInFreeJumpRange = true;
                        }
                    }
                }
                //                else
                {
                    // deselect after timeout

                }
            }
            else
            {
                if (selectedIsland != null)
                    RemoveSelectionArrow();
            }

            if ((selectedIsland != null
            && at > islandSelectedAt + constants.GetFloat("island_deselection_timeout")))
            {
                RemoveSelectionArrow();
            }
       
            if (selectedIsland != null)
            {
                // check range
                Vector3 xzdist = selectedIsland.GetVector3("position") - playerPosition;
                xzdist.Y = 0;
                if (xzdist.Length() < constants.GetFloat("island_jump_free_range")
                    || player.GetInt("jumps") > 0) // arrow indicates jump 
                {
                    //arrow.SetVector2("persistent_squash", new Vector2(100f, 1f));
//                    (arrow.GetProperty("render") as ArrowRenderProperty).JumpPossible = true;//SquashParams = new Vector2(100f, 1f);
                    // hack hack hack
//                    if(xzdist.Length() < constants.GetFloat("island_jump_free_range"))
                        selectedIslandInFreeJumpRange = true;
                }
                else
                {
                    //arrow.SetVector2("persistent_squash", new Vector2(1000f, 0.8f));
                    arrow.GetProperty<ArrowRenderProperty>("render").JumpPossible = false;//SquashParams = new Vector2(1000f, 0.8f);
                }
            }
        }

        private void RemoveSelectionArrow()
        {
            selectedIsland = null;
            if(arrow.HasProperty("render"))
                arrow.RemoveProperty("render");
            if (arrow.HasProperty("shadow_cast"))
                arrow.RemoveProperty("shadow_cast");
            arrow.SetString("island", "");
            islandSelectionLastStickDir = Vector3.Zero;
            islandSelectedAt = 0;
        }

        // todo: move up this
        private Vector3 islandRepulsionStartDir;
        private Quaternion islandRepulsionStartRotation;
        private bool repulsionPossible = false;
        private float crawlStateChangedAt = 0;

        private void PerformIslandRepulsionAction(SimulationTime simTime, ref int fuel)
        {
            if (activeIsland != null)
            {
                // island repulsion start
                if (!repulsionActive
                    && (controllerInput.repulsionButtonPressed
                    || controllerInput.repulsionButtonHold)
                    && activeIsland.GetString("repulsed_by") == ""
                    && player.GetInt("energy") > constants.GetInt("island_repulsion_start_min_energy"))
                {
                    if (controllerInput.moveStickMoved)
                    {
                        islandRepulsionStartDir = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));
                        islandRepulsionLastStickDir = islandRepulsionStartDir;
                        islandRepulsionStartRotation = activeIsland.GetQuaternion("rotation");

                        Vector3 currentVelocity = activeIsland.GetVector3("repulsion_velocity");
                        Vector3 velocity = islandRepulsionStartDir *
                            constants.GetFloat("island_repulsion_start_velocity_multiplier");
                        activeIsland.SetVector3("repulsion_velocity", currentVelocity + velocity);

                        player.SetInt("energy", player.GetInt("energy") - constants.GetInt("island_repulsion_start_energy_cost"));

                        StartRepulsion();
                    }
                    else
                        if(!repulsionPossible)
                        {
                            // if button pressed but move stick not moved just indicate possible stuff
                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "repulsion";
                            player.GetProperty<HUDProperty>("hud").RepulsionUsable = true;
                            repulsionPossible = true;
                        }
                }
                else
                    // island repulsion
                    if (repulsionActive
                        && controllerInput.repulsionButtonHold
                        && activeIsland != null
                        && player.GetInt("energy") > 0
                        )
                    {
                        Vector3 dir = new Vector3(controllerInput.leftStickX, 0, controllerInput.leftStickY);
                        if (dir != Vector3.Zero)
                        {
                            dir.Normalize();
                            player.GetProperty<HUDProperty>("hud").RepulsionUsable = false;
                        }
                        else
                        {
                            player.GetProperty<HUDProperty>("hud").RepulsionUsable = true;
                        }

                        if (simTime.At > crawlStateChangedAt + 200) // todo: change constant
                        {
                            if (dir == islandRepulsionLastStickDir)
                            {
                                player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "repulsion";
                                crawlStateChangedAt = simTime.At;
                            }
                            else
                                if (Vector3.Cross(dir, islandSelectionLastStickDir).Y > 0)
                                {
                                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "crawl_left";
                                    crawlStateChangedAt = simTime.At;
                                }
                                else
                                {
                                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "crawl_right";
                                    crawlStateChangedAt = simTime.At;
                                }
                        }
                        islandRepulsionLastStickDir = dir;
                        /*
                        if(dir != Vector3.Zero)
                        {
                            Matrix rotationStart = Matrix.CreateFromQuaternion(islandRepulsionStartRotation);
                            float yRotation = (float)Math.Atan2(controllerInput.leftStickX, controllerInput.leftStickY);
                            Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                            activeIsland.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationStart * rotationMatrix));
                        }
                        */


                        Vector3 currentVelocity = activeIsland.GetVector3("repulsion_velocity");
                        activeIsland.SetVector3("repulsion_velocity", currentVelocity +
                            dir * constants.GetFloat("island_repulsion_acceleration") * simTime.Dt);

                        Game.Instance.Simulation.ApplyPerSecondSubstraction(player, "repulsion_energy_cost",
                            constants.GetInt("island_repulsion_energy_cost_per_second"), player.GetIntAttribute("energy"));

                        if (player.GetInt("energy") <= 0)
                        {
                            StopRepulsion();
                        }
                    }
                    else
                        if (repulsionActive)
                        {
                            StopRepulsion();
                        }
                        else
                        if(repulsionPossible)
                        {
                            // removed idicators
                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                            player.GetProperty<HUDProperty>("hud").RepulsionUsable = false;
                            repulsionPossible = false;
                        }
            }
            else
                if(repulsionActive)
                {
                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                    player.GetProperty<HUDProperty>("hud").RepulsionUsable = false;
                    repulsionPossible = false;
                    repulsionActive = false;
                }
        }

        private void StartRepulsion()
        {
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "repulsion";
            activeIsland.SetString("repulsed_by", player.Name);
            repulsionActive = true;
        }

        private void StopRepulsion()
        {
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
            player.GetProperty<HUDProperty>("hud").RepulsionUsable = false;
            activeIsland.SetString("repulsed_by", "");
            repulsionActive = false;
            repulsionPossible = false;
        }

        private void PerformFlamethrowerAction(Entity player, ref Vector3 playerPosition)
        {
            // flamethrower
            if (((controllerInput.flamethrowerButtonPressed
                || controllerInput.flamethrowerButtonHold)
                && !RightStickFlame)
                || (controllerInput.flameStickMoved && RightStickFlame)
                && player.GetInt("frozen") <= 0 // not allowed when frozen
                && activeIsland != null) // only allowed on ground
            {
                if (flame == null)
                {
                    if (player.GetInt("energy") > constants.GetInt("flamethrower_warmup_energy_cost"))
                    {
                        // indicate 
                        flameThrowerSoundInstance = flameThrowerSound.Play(Game.Instance.EffectsVolume, 1, 0, true);

                        Vector3 pos = playerPosition + constants.GetVector3("flamethrower_offset");
                        Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                        flame = new Entity("flame" + "_" + player.Name);
                        flame.AddStringAttribute("player", player.Name);

                        flame.AddVector3Attribute("velocity", viewVector);
                        flame.AddVector3Attribute("position", pos);
                        flame.AddQuaternionAttribute("rotation", GetRotation(player));

                        Game.Instance.Simulation.EntityManager.AddDeferred(flame, "flamethrower_base", templates);

                        // indicate on model
                        player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "attack_long";
                    }
                }
                else
                {
                    // only rotate when fueled
                    if (flame.GetBool("fueled"))
                    {
                        // change y rotation towards player in range
                        Vector3 aimVector;
                        Vector3 offsetDir = Vector3.Normalize(constants.GetVector3("flamethrower_offset"));
                        Vector3 flamePos = flame.GetVector3("position");
                        Vector3 direction = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));
                        float maxAngle = constants.GetFloat("flamethrower_aim_angle");
                        Entity targetPlayer = SelectBestPlayerInDirection(ref playerPosition, ref direction, maxAngle, out aimVector);
                        if (targetPlayer != null)
                        {
                            // correct target vector based on flame positon
                            Vector3 targetPos = targetPlayer.GetVector3("position") + Vector3.UnitY * targetPlayer.GetVector3("scale").Y / 2;
                            aimVector = targetPlayer.GetVector3("position") - flamePos;
                            if (aimVector != Vector3.Zero)
                                aimVector.Normalize();

                            // correct direciton based on flame position
                            direction = direction - offsetDir;
                            if (direction != Vector3.Zero)
                                direction.Normalize();

                            // get angle between stick dir and aimVector 
                            float a = (float)(Math.Acos(Vector3.Dot(aimVector, direction)) / Math.PI * 180);
                            float dirM = a / maxAngle;
                            float aimM = 1 - dirM;

                            //                            Console.WriteLine("giving " + aimM + " support.");

                            // we only aim perfectly in y, but give a little support in x/z
                            aimVector.X = direction.X * dirM + aimVector.X * aimM;
                            aimVector.Z = direction.Z * dirM + direction.Z * aimM;
                        }
                        else
                        {
                            // correct direciton based on flame position
                            direction = direction - offsetDir;
                            if (direction != Vector3.Zero)
                                direction.Normalize();
                        }

                        Vector3 tminusp = aimVector;
                        Vector3 ominusp = Vector3.Backward;
                        if (tminusp != Vector3.Zero)
                            tminusp.Normalize();
                        float theta = (float)System.Math.Acos(Vector3.Dot(tminusp, ominusp));
                        Vector3 cross = Vector3.Cross(ominusp, tminusp);

                        if (cross != Vector3.Zero)
                            cross.Normalize();

                        Quaternion targetQ = Quaternion.CreateFromAxisAngle(cross, theta);

                        flame.SetQuaternion("rotation", targetQ);

                        // stop when energy runs out
                        if (player.GetInt("energy") <= 0)
                        {
                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                            flame.SetBool("fueled", false);
                        }
                    }
                }
            }
            else
            {
                if (flame != null)
                {
                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                    flame.SetBool("fueled", false);
                }
            }
        }

        private void PerformIceSpikeAction(Entity player, float at, Vector3 playerPosition)
        {
            // ice spike
            if (controllerInput.iceSpikeButtonPressed && player.GetInt("energy") > constants.GetInt("ice_spike_energy_cost")
                && (at - iceSpikeFiredAt) > constants.GetInt("ice_spike_cooldown"))
            {
                // indicate 
                SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>("Sounds/hit2");
                soundEffect.Play(Game.Instance.EffectsVolume);

                Vector3 pos = new Vector3(playerPosition.X, playerPosition.Y + player.GetVector3("scale").Y, playerPosition.Z);

                Vector3 aimVector;
                Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));
                Entity targetPlayer = SelectBestPlayerInDirection(ref playerPosition, ref viewVector, 
                    constants.GetFloat("ice_spike_aim_angle"), out aimVector);
                String targetPlayerName = targetPlayer != null ? targetPlayer.Name : "";

                Entity iceSpike = new Entity("icespike" + (++iceSpikeCount) + "_" + player.Name);
                iceSpike.AddStringAttribute("player", player.Name);
                iceSpike.AddStringAttribute("target_player", targetPlayerName);
                if (targetPlayer == null)
                    iceSpike.AddVector3Attribute("target_direction", aimVector);
                iceSpike.AddIntAttribute("creation_time", (int)at);

                Vector3 initVelocity = aimVector;
                initVelocity.Y = 1;
                initVelocity *= constants.GetVector3("ice_spike_initial_speed_multiplier");

                iceSpike.AddVector3Attribute("velocity", initVelocity);
                iceSpike.AddVector3Attribute("position", pos);

                iceSpike.AddStringAttribute("bv_type", "sphere");

                Game.Instance.Simulation.EntityManager.AddDeferred(iceSpike, "ice_spike_base", templates);

                // update states
                player.SetInt("energy", player.GetInt("energy") - constants.GetInt("ice_spike_energy_cost"));
                iceSpikeFiredAt = at;
            }
        }

        private void PerformFrozenSlowdown(Entity player, SimulationTime simTime, ref Vector3 playerPosition)
        {
            if (player.GetInt("frozen") > 0
                && activeIsland != null) // only when on island...
            {
                Vector3 add = playerPosition - previousPosition;
                playerPosition = previousPosition + add / constants.GetFloat("frozen_slowdown_divisor");
                player.SetInt("frozen", player.GetInt("frozen") - (int)simTime.DtMs);
                if (player.GetInt("frozen") < 0)
                    player.SetInt("frozen", 0);
            }
        }

        private void PerformStickMovement(Entity player, float dt, ref Vector3 playerPosition, int fuel)
        {
            if (destinationIsland != null)
            {
                // no movement during jump
                return;
            }

            if (controllerInput.moveStickMoved)
            {
                movedAt = Game.Instance.Simulation.Time.At;

                // XZ movement
                if (activeIsland == null)
                {
                    // in air
                    playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_jetpack_multiplier");
                    playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_jetpack_multiplier");
                }
                else
                    // don't allow positioning on island if hit
                    if (player.GetVector3("hit_pushback_velocity") == Vector3.Zero
                        && !controllerInput.flamethrowerButtonHold
                        && !controllerInput.repulsionButtonHold)
                    {
                        // on island ground
                        if (controllerInput.runButtonHold)
                        {
                            Game.Instance.Simulation.ApplyPerSecondSubstraction(player, "running_energy_cost",
                                constants.GetInt("running_energy_cost_per_second"), player.GetIntAttribute("energy"));

                            playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_run_multiplier");
                            playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_run_multiplier");

                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "run";
                        }
                        else
                        {
                            playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_walk_multiplier");
                            playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_walk_multiplier");

                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "walk";
                        }

                        // check position a bit further in walking direction to be still on island
                        float islandNonWalkingRangeMultiplier = constants.GetFloat("island_non_walking_range_multiplier");
                        Vector3 checkPos = playerPosition + new Vector3(controllerInput.leftStickX * islandNonWalkingRangeMultiplier,
                            0, controllerInput.leftStickY * islandNonWalkingRangeMultiplier); // todo: extract constant

                        Vector3 isectPt;
                        if (!Simulation.GetPositionOnSurface(ref checkPos, activeIsland, out isectPt))
                        {
                            // check point outside of island -> prohibit movement
                            playerPosition = previousPosition;
                        }
                    }
                
                // rotation
                if (destinationIsland == null)
                {
                    float yRotation = (float)Math.Atan2(controllerInput.leftStickX, controllerInput.leftStickY);
                    Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                    player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));
                }
            }
            else
            {
                if(player.GetProperty<RobotRenderProperty>("render").NextPermanentState == "walk"
                    || player.GetProperty<RobotRenderProperty>("render").NextPermanentState == "run")
                {
                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                }
            }

            if (activeIsland != null)
            {
                // position on island surface
                Vector3 isectPt = Vector3.Zero;
                if (Simulation.GetPositionOnSurface(ref playerPosition, activeIsland, out isectPt))
                {
                    // check height difference
                    /*
                    if (isectPt.Y - previousPosition.Y > 10)
                    {
                        playerPosition = previousPosition;
                    }
                    else
                     */
                    // todo: not in future
//                    if (player.GetVector3("hit_pushback_velocity") == Vector3.Zero)
                    {
                        // set position to contact point
                        playerPosition.Y = isectPt.Y + 1; // todo: make constant?
                    }
                }
                else
                // not over island anymore
                {
                    LeaveActiveIsland();
                }
            }
        }

        private void ApplyGravity(float dt, ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            if (activeIsland == null
                && destinationIsland == null)
            {
                // gravity
                if (playerVelocity.Length() <= constants.GetFloat("max_gravity_speed")
                    || playerVelocity.Y > 0) // gravity max speed only applies for downwards speeds
                {
                    // todo: hack hack hack
                    if (simpleJumpIsland != null)
                    {
                        playerVelocity += constants.GetVector3("simple_jump_gravity_acceleration") * dt;
                    }
                    else
                    {
                        playerVelocity += constants.GetVector3("gravity_acceleration") * dt;
                    }
                }
            }
        }

        private void PerformIslandJump(Entity player, float dt, float at, ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            if (destinationIsland != null)
            {
                // apply from last jump
                playerPosition += player.GetVector3("island_jump_velocity") * dt;

                // get current
                Vector3 landingPos = GetLandingPosition(destinationIsland);
                Vector3 islandDir = landingPos - playerPosition;
                islandDir.Y = 0;

                // calculate y position 
                float r = 1 - (islandDir.Length() / destinationOrigDist);
                if (r < 0)
                    r = 0;
                if (r > 1)
                    r = 1;
                float S = constants.GetFloat("island_jump_arc_height");
                float or = 1 - r;
                float mid = Math.Max(destinationOrigY + S, landingPos.Y + S);
                // bezier
                float y = destinationOrigY * or * or + 2 * mid * or * r + landingPos.Y * r * r;
                playerPosition.Y = y;
                // Console.WriteLine("y: "+y);

                // check for arrival
                Vector3 isectPt = Vector3.Zero;
                Ray3 ray = new Ray3(playerPosition + 1000 * Vector3.UnitY, -Vector3.UnitY);
                if ((islandDir.Length() < 4 // near enough...
                    || Vector3.Dot(islandDir, lastIslandDir) < 0) // or oscilation)
                   && Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, destinationIsland, out isectPt))
                {
                    SetActiveIsland(destinationIsland);

                    player.GetProperty<BasicRenderProperty>("render").Squash();
                    destinationIsland.GetProperty<IslandRenderProperty>("render").Squash();

                    playerPosition = isectPt;

                    StopIslandJump();
                    playerVelocity = Vector3.Zero;
                    islandJumpPerformedAt = at;
                }
                else
                {
                    // not yet -> calculate next velocity for jump
                    float yRotation = (float)Math.Atan2(islandDir.X, islandDir.Z);
                    Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                    player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));

                    lastIslandDir = islandDir;

                    float speed = destinationOrigDist * 2.2f;
                    if (speed < 300)
                        speed = 300;
                    if (islandDir != Vector3.Zero)
                        islandDir.Normalize();
                    player.SetVector3("island_jump_velocity", islandDir * speed);
                }
            }
        }

        private void StopIslandJump()
        {
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
            destinationIsland.GetAttribute<Vector3Attribute>("position").ValueChanged -= IslandPositionHandler;
            destinationIsland = null;
        }

        private void PerformJetpackMovement(SimulationTime simTime, float dt, ref Vector3 playerVelocity, ref int fuel)
        {
            if ((controllerInput.jetpackButtonPressed
                || controllerInput.jetpackButtonHold)
                && activeIsland == null // only in air
                && destinationIsland == null // not while jump
                && simpleJumpIsland == null // dito
                && flame == null // not in combination with flame
                && fuel > 0 // we need fuel
                && player.GetInt("frozen") <= 0 // jetpack doesn't work when frozen
            )
            {
                player.GetProperty<HUDProperty>("hud").JetpackUsable = false;

                if (!jetpackActive)
                {
                    jetpackSoundInstance = jetpackSound.Play(0.4f * Game.Instance.EffectsVolume, 1, 0, true);
                    jetpackActive = true;
                }

                //                fuel -= (int)simTime.DtMs; // todo: add constant that can modify this
                playerVelocity += constants.GetVector3("jetpack_acceleration") * dt;

                // deaccelerate the higher we get
                // todo: extract constants (450 & 5 & 10)
                float dist = 480 - player.GetVector3("position").Y;
                Vector3 deacceleration = constants.GetVector3("jetpack_acceleration") * 6 / dist;
                if (dist < 10)
                    playerVelocity = Vector3.Zero;
                else
                    playerVelocity -= deacceleration * dt;

                // ensure we're not to fast
                if (playerVelocity.Length() > constants.GetFloat("max_jetpack_speed"))
                {
                    playerVelocity.Normalize();
                    playerVelocity *= constants.GetFloat("max_jetpack_speed");
                }
            }
            else
            {
                if (jetpackActive)
                {
                    jetpackSoundInstance.Stop();
                    jetpackActive = false;
                }
            }
        }

        /// <summary>
        /// checks if player's health has fallen below 0, then perform respawn (if he has any lifes left)
        /// </summary>
        /// <returns>wheter the player is currently dead or not</returns>
        private bool CheckAndPerformDeath(Entity player, float at)
        {
            if (player.GetInt("health") <= 0)
            {
                if (respawnStartedAt == 0)
                {
                    respawnStartedAt = at;

                    if (jetpackSoundInstance != null)
                        jetpackSoundInstance.Stop();
                    if (flameThrowerSoundInstance != null)
                        flameThrowerSoundInstance.Stop();
                    jetpackActive = false;
                    if (destinationIsland != null)
                        StopIslandJump();
                    if (repulsionActive)
                        StopRepulsion();
                    LeaveActiveIsland();
                    if(simpleJumpIsland != null)
                        StopSimpleJump();

                    Game.Instance.ContentManager.Load<SoundEffect>("Sounds/death").Play(Game.Instance.EffectsVolume);
                    player.SetInt("deaths", player.GetInt("deaths") + 1);
                    player.SetInt("lives", player.GetInt("lives") - 1);

                    player.GetProperty<CollisionProperty>("collision").OnContact -= PlayerCollisionHandler;

                    // deactivate
                    player.RemoveProperty("render");
                    player.RemoveProperty("shadow_cast");
                    player.RemoveProperty("collision");

                    if (selectedIsland != null)
                    {
                        RemoveSelectionArrow();
                    }

                    if (flame != null)
                    {
                        // remove flamethrower flame
                        Game.Instance.Simulation.EntityManager.RemoveDeferred(flame);
                        flame = null;
                    }

                    // explode!

                    // remove old explosion if still there
                    if (deathExplosion != null
                        && Game.Instance.Simulation.EntityManager.ContainsEntity(deathExplosion))
                    {
                        Game.Instance.Simulation.EntityManager.RemoveDeferred(deathExplosion);
                    }

                    // add explosion
                    deathExplosion = new Entity(player.Name + "_explosion");
                    deathExplosion.AddStringAttribute("player", player.Name);

                    Vector3 pos = player.GetVector3("position");               
                    deathExplosion.AddVector3Attribute("position", pos);

                    Game.Instance.Simulation.EntityManager.AddDeferred(deathExplosion, "player_explosion_base", templates);

                    // any lives left?
                    if (player.GetInt("lives") <= 0)
                    {
                        Game.Instance.Simulation.EntityManager.EntityRemoved -= EntityRemovedHandler;
                        Game.Instance.Simulation.EntityManager.RemoveDeferred(player);
                    }

                    // dead
                    return true;
                }
                else
                    if (respawnStartedAt + constants.GetInt("respawn_time") >= at)
                    {
                        // still dead
                        return true;
                    }
                    else
                    {
                        // random island selection
                        PositionOnRandomIsland();

                        // reset
                        player.SetQuaternion("rotation", Quaternion.Identity);
                        player.SetVector3("velocity", Vector3.Zero);

                        player.SetVector3("collision_pushback_velocity", Vector3.Zero);
                        player.SetVector3("player_pushback_velocity", Vector3.Zero);
                        player.SetVector3("hit_pushback_velocity", Vector3.Zero);

                        player.SetInt("energy", constants.GetInt("max_energy"));
                        player.SetInt("health", constants.GetInt("max_health"));
                        player.SetInt("fuel", constants.GetInt("max_fuel"));

                        player.SetInt("frozen", 0);

                        // activate
                        player.AddProperty("collision", new CollisionProperty());
                        player.AddProperty("render", new RobotRenderProperty());
                        player.AddProperty("shadow_cast", new ShadowCastProperty());
                        player.GetProperty<CollisionProperty>("collision").OnContact += PlayerCollisionHandler;

                        // reset respawn timer
                        respawnStartedAt = 0;

                        // alive again
                        return false;
                    }
            }

            // not dead
            return false;
        }

        public void CheckPlayerAttributeRanges(Entity player)
        {
            int health = player.GetInt("health");
            if(health < 0)
                player.SetInt("health", 0);
            else
                if (health > constants.GetInt("max_health"))
                    player.SetInt("health", constants.GetInt("max_health"));

            int energy = player.GetInt("energy");
            if (energy < 0)
                player.SetInt("energy", 0);
            else
                if (energy > constants.GetInt("max_energy"))
                    player.SetInt("energy", constants.GetInt("max_energy"));

            int fuel = player.GetInt("fuel");
            if (fuel < 0)
                player.SetInt("fuel", 0);
            else
            if (fuel > constants.GetInt("max_fuel"))
                player.SetInt("fuel", constants.GetInt("max_fuel"));

            if (player.GetInt("jumps") < 0)
                player.SetInt("jumps", 0);
            if (player.GetFloat("repulsion_seconds") < 0)
                player.SetFloat("repulsion_seconds", 0);
        }

        private Vector3 GetLandingPosition(Entity island)
        {
            int gpi = player.GetInt("game_pad_index");
            Vector3 pos;
            if (island.HasAttribute("landing_offset_p" + gpi))
            {
                pos = island.GetVector3("position") + island.GetVector3("landing_offset_p" + gpi);
            }
            else
            {
                pos = island.GetVector3("position") + island.GetVector3("landing_offset");
            }
            return pos;
        }

        private void PlayerCollisionHandler(SimulationTime simTime, Contact contact)
        {
            if (contact.EntityB.HasAttribute("kind"))
            {
                // slide on jump
                if (destinationIsland != null)
                {
                    // hack: no response on jump (who sees that anyway...)
                    return;

                    if (contact.EntityB == destinationIsland)
                        return;

                    Vector3 position = destinationIsland.GetVector3("position");

                    // get distance of destination point to sliding plane
                    float distance = Vector3.Dot(contact[0].Normal, position - contact[0].Point);

                    // calculate point on plane
                    Vector3 cpos = position - contact[0].Normal * distance;

                    Vector3 slidingDir = Vector3.Normalize(cpos - contact[0].Point);
                    Vector3 slidingVelocity = slidingDir * constants.GetFloat("island_jump_speed");

//                    Console.WriteLine("orignal velocity " + player.GetVector3("island_jump_velocity") + "; sliding velocity: " + slidingVelocity);

                    //                    slidingVelocity.Y = player.GetVector3("island_jump_velocity").Y;
//                    slidingVelocity.Y = player.GetVector3("island_jump_velocity").Y;
//                    player.SetVector3("island_jump_velocity", slidingVelocity);

                    // also ensure we don't fall down yet
//                    player.SetVector3("velocity", player.GetVector3("velocity") - constants.GetVector3("gravity_acceleration") * simTime.Dt);

                    return;
                }

                String kind = contact.EntityB.GetString("kind");
                switch (kind)
                {
                    case "island":
                        PlayerIslandCollisionHandler(simTime, contact.EntityA, contact.EntityB, contact);
                        break;
                    case "pillar":
                        PlayerPillarCollisionHandler(simTime, contact.EntityA, contact.EntityB, contact);
                        break;
                    case "cave":
                        PlayerCaveCollisionHandler(simTime, contact.EntityA, contact.EntityB, contact);
                        break;
                    case "player":
                        PlayerPlayerCollisionHandler(simTime, contact.EntityA, contact.EntityB, contact);
                        break;
                }
                CheckPlayerAttributeRanges(player);
                collisionAt = simTime.At;
            }
        }

        private void PlayerIslandCollisionHandler(SimulationTime simTime, Entity player, Entity island, Contact contact)
        {
            float dt = simTime.Dt;
            if (island == activeIsland)
            {
                // don't do any collision reaction with island we are standing/jumping on
                return;
            }

            // no collision response at all
            if (ImuneToIslandPush
                && activeIsland != null)
            {
                return;
            }

            // get theoretical position on island
            Vector3 playerPosition = player.GetVector3("position");
            Vector3 surfacePosition;
            Simulation.GetPositionOnSurface(ref playerPosition, island, out surfacePosition);

            // on top?
            // todo: extract constant
            if ((surfacePosition.Y - 5 < playerPosition.Y
                && activeIsland == null) // don't allow switching of islands
                || island == simpleJumpIsland)
            {
//                Console.WriteLine(player.Name + " collidet with " + island.Name);

                if (destinationIsland != null) // if we are in jump, don't active island
                {
                    return;
                }

                if (island == simpleJumpIsland)
                {
                    if (player.GetVector3("velocity").Y > 0)
                    {
                        // don't do anything at begining of jump
                        return;
                    }
                    else
                    {
                        // remove handler
                        StopSimpleJump();
                    }
                }

                // has active island changed (either from none or another)
                if (activeIsland != island)
                {
                    // leave old
                    LeaveActiveIsland();
                    // set new
                    SetActiveIsland(island);
                }

                // stop falling
                player.SetVector3("velocity", Vector3.Zero);

                // position
                Vector3 isectPt;
                if (Simulation.GetPositionOnSurface(ref playerPosition, island, out isectPt))
                {
                    // set position to contact point
                    playerPosition.Y = isectPt.Y + 1;
                    player.SetVector3("position", playerPosition);
                }
            }
            else
            {
                Vector3 pos = player.GetVector3("position");
                if (activeIsland != null)
                {
                    // on island -> calculate pseudo normal in xz
                    Vector3 normal = island.GetVector3("position") - pos;
                    normal.Y = 0;
                    if (normal != Vector3.Zero)
                    {
                        Vector3 velocity = -normal * 100; // todo: extract constant
                        player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
                    }
                }
                else
                if (simpleJumpIsland != null)
                {
                    // don't push in xz if in simplejump
                    Vector3 normal = island.GetVector3("position") - pos;
                    normal.X = 0;
                    normal.Z = 0;
                    if (normal != Vector3.Zero)
                    {
                        Vector3 velocity = -normal * 100; // todo: extract constant
                        player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
                    }
                }
                else
                {
                    // in air --> pushback
                    Vector3 velocity = -contact[0].Normal * 1000; // todo: extract constant
                    if (velocity.Y > 0) // hack: never push upwards
                        velocity.Y = -velocity.Y;
                    player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
                }
            }
        }

        private void PlayerPillarCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            // only collide with pillar when in air
            if (activeIsland == null)
            {
                Vector3 normal = pillar.GetVector3("position") - player.GetVector3("position");
                normal.Y = 0;
                if (normal != Vector3.Zero)
                    normal.Normalize();
                // todo: extract constant
                Vector3 velocity = -normal * 1000;
                player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
            }
        }

        private void PlayerCaveCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            // only collide with cave when in air
            if (activeIsland == null)
            {
                Vector3 pos = player.GetVector3("position");
                // todo: extract constants
                Vector3 velocity = (-Vector3.Normalize(pos) - Vector3.UnitY / 10) * 3000;
                velocity.Y = 0;
                player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
            }
        }

        private void PlayerLavaCollisionHandler(SimulationTime simTime, Entity player, Entity lava)
        {
            if (player.GetVector3("velocity").Y < 0) // stop downwards velocity
            {
                Vector3 velocity = player.GetVector3("velocity");
                // todo: extract constant
                player.SetVector3("velocity", velocity - velocity * 0.9f * simTime.Dt * 25);
            }
            Game.Instance.Simulation.ApplyPerSecondSubstraction(player, "lava_damage", constants.GetInt("lava_damage_per_second"),
                player.GetIntAttribute("health"));
        }

        private void PlayerPlayerCollisionHandler(SimulationTime simTime, Entity player, Entity otherPlayer, Contact c)
        {
            Vector3 normal = otherPlayer.GetVector3("position") - player.GetVector3("position");
            normal.Y = 0;
            if(normal != Vector3.Zero)
                normal.Normalize();

            // and hit?
            if (simTime.At < controllerInput.hitButtonPressedAt + constants.GetInt("hit_cooldown") // has button been pressed lately
                && simTime.At > hitPerformedAt + constants.GetInt("hit_cooldown") // but no hit performed
                && player.GetVector3("hit_pushback_velocity") == Vector3.Zero) // and we have not been hit ourselves
            {
                // indicate hit!
                SoundEffect soundEffect = Game.Instance.ContentManager.Load<SoundEffect>("Sounds/punch2");
                soundEffect.Play(Game.Instance.EffectsVolume);

                // deduct health
                otherPlayer.SetInt("health", otherPlayer.GetInt("health") - constants.GetInt("hit_damage"));
                CheckPlayerAttributeRanges(otherPlayer);

                // set values
                // todo: bring back up vector! -> needs change in psotion in island too
                Vector3 velocity = (normal /*+ Vector3.UnitY*/) * constants.GetVector3("hit_pushback_velocity_multiplier");
                otherPlayer.SetVector3("hit_pushback_velocity", velocity);
                otherPlayer.SetVector3("position", otherPlayer.GetVector3("position") + velocity * simTime.Dt);
                hitPerformedAt = simTime.At;

                // indicate in model
                otherPlayer.GetProperty<RobotRenderProperty>("render").NextOnceState = "pushback";
            }
            else
            {
                // apply collision response to moving player
                if ((simTime.At < movedAt + 400 // todo: extract constants
                    || simTime.At < islandJumpPerformedAt + 1000)
                    && otherPlayer.GetVector3("hit_pushback_velocity") == Vector3.Zero) // player which was hit doesn't push us back
                {
                    // calculate pseudo-radi
                    float pr = (player.GetVector3("scale") * new Vector3(1, 0, 1)).Length();
                    float or = (otherPlayer.GetVector3("scale") * new Vector3(1, 0, 1)).Length();
                    float dist = ((player.GetVector3("position") - otherPlayer.GetVector3("position")) * new Vector3(1, 0, 1)).Length();
                    float delta = or + pr - dist;

                    // Console.WriteLine("putout delta: " + delta);
                    if (delta < 0)
                        delta = 0;

                    player.SetVector3("position", player.GetVector3("position") - normal * delta);
                }

//                if (movedByStick && activeIsland != null)
//                {
//                    player.SetVector3("position", previousPosition);
//                    player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity")
//                        - normal * 100);
//                }
//                else
//                if (movedByStick || activeIsland == null)
//                {
//                    // normal feedback
//                    //                    if (Vector3.Dot(dir, normal) > 0 // and only if normal faces right direction
////                        || dir == Vector3.Zero)
//                    {
//                        player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity")
//                            - normal * 100);
//                    }

//                    /*
//                    Vector3 position = otherPlayer.GetVector3("position");

//                    // get distance of destination point to sliding plane
//                    float distance = Vector3.Dot(c[0].Normal, position - c[0].Point);

//                    // calculate point on plane
//                    Vector3 cpos = position - c[0].Normal * distance;

//                    Vector3 slidingDir = Vector3.Normalize(cpos - c[0].Point);
//                    Vector3 slidingVelocity = slidingDir * (previousPosition-player.GetVector3("position")).Length();
//                    slidingVelocity.Y = 0;

//                    player.SetVector3("position", previousPosition + slidingVelocity);
//                     */
//                }
            }
        }

        private void IslandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = player.GetVector3("position");
            Vector3 delta = newValue - oldValue;
            position += delta;
            previousPosition += delta;
            player.SetVector3("position", position);
        }

        private void HealthChangeHandler(IntAttribute sender, int oldValue, int newValue)
        {
            if (newValue < oldValue
                && oldValue <= constants.GetInt("max_health")
                && oldValue > 0) // and not dead yet
            {
                // damage taken
                if (oldValue - newValue >= constants.GetInt("ice_spike_damage"))
                {
                    player.GetProperty<RobotRenderProperty>("render").NextOnceState = "hurt_hard";
                }
                else
                {
                    player.GetProperty<RobotRenderProperty>("render").NextOnceState = "hurt_soft";
                }
            }
        }

        private void EntityRemovedHandler(AbstractEntityManager<Entity> manager, Entity entity)
        {
            if (entity.Name.Equals("flame" + "_" + player.Name))
            {
                if(flameThrowerSoundInstance != null
                    && !flameThrowerSoundInstance.IsDisposed)
                    flameThrowerSoundInstance.Stop();
                flame = null;
                return;
            }

            if (entity.HasAttribute("kind")
                && entity.GetString("kind") == "icespike"
                && entity.GetString("player").Equals(player.Name))
                iceSpikeRemovedCount++;

            // reset count to 0 if all spikes gone
            if (iceSpikeCount == iceSpikeRemovedCount)
            {
                iceSpikeCount = 0;
                iceSpikeRemovedCount = 0;
            }

            // check removed entity wasn't island we were using
            if (entity == selectedIsland)
            {
                RemoveSelectionArrow();
            }
            if (entity == destinationIsland)
            {
                destinationIsland = null;
            }
            if (entity == activeIsland)
            {
                activeIsland = null;
                player.SetString("active_island", "");
            }

            // check if player was removed (lost all his lives)
            if(entity.HasAttribute("kind") && entity.GetString("kind") == "player")
            {
                // check if we are last man standing
                if (Game.Instance.Simulation.PlayerManager.Count == 1 && 
                    (Game.Instance.Simulation.Phase == SimulationPhase.Intro || Game.Instance.Simulation.Phase == SimulationPhase.Game))
                {
                    won = true;

                    if (player.HasProperty("render")
                        && activeIsland != null)
                    {
                        player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "win";
                    }

                    if (selectedIsland != null)
                    {
                        RemoveSelectionArrow();
                    }

                    if (flame != null)
                    {
                        // remove flamethrower flame
                        Game.Instance.Simulation.EntityManager.RemoveDeferred(flame);
                        flame = null;
                    }

                    Entity winningPlayer = Game.Instance.Simulation.PlayerManager[0];

                    Game.Instance.Simulation.SetPhase(
                        SimulationPhase.Outro,
                        winningPlayer.GetString("player_name"),
                        null
                        );
                }
            }
        }

        /// <summary>
        /// selects and island closest to direction dir
        /// </summary>
        /// <param name="dir">direction to select</param>
        /// <returns>an island entity or null</returns>
        private Entity SelectBestIslandInDirection(ref Vector3 dir)
        {
            float maxAngle = constants.GetFloat("island_aim_angle");
            float closestAngle = float.MaxValue;
            float distance = float.MaxValue;
            Entity selectedIsland = null;
            foreach (Entity island in Game.Instance.Simulation.IslandManager)
            {
                if (island == activeIsland) // never select active island
                    continue;

                Vector3 islandDir = island.GetVector3("position") - player.GetVector3("position");
                float dist = islandDir.Length();
                islandDir.Y = 0;
                float xzdist = islandDir.Length();
                float angle = (float)(Math.Acos(Vector3.Dot(dir, islandDir) / dist) / Math.PI * 180);
                if (xzdist < constants.GetFloat("island_jump_free_range"))
                {
                    if (island != activeIsland
                        && angle < maxAngle)
                    {
                        if (angle < closestAngle
                            || (Math.Abs(angle - closestAngle) < constants.GetFloat("island_aim_angle_eps")
                            && dist < distance))
                        {
                            selectedIsland = island;
                            closestAngle = angle;
                            distance = dist;
                        }
                    }
                }
            }

            return selectedIsland;
        }

        /// <summary>
        /// selects the player best fitting direction given
        /// </summary>
        private Entity SelectBestPlayerInDirection(ref Vector3 playerPosition, ref Vector3 direction, float maxAngle, out Vector3 aimVector)
        {
            float minAngle = float.PositiveInfinity;
            Entity targetPlayer = null;
            aimVector = direction;
            foreach (Entity p in Game.Instance.Simulation.PlayerManager)
            {
                if (p != player)
                {
                    Vector3 pp = p.GetVector3("position");
                    Vector3 pdir = pp - playerPosition;
                    float a = (float)(Math.Acos(Vector3.Dot(pdir, direction) / pdir.Length()) / Math.PI * 180);
                    if (a < maxAngle)
                    {
                        if (a < minAngle)
                        {
                            targetPlayer = p;
                            aimVector = pdir + Vector3.UnitY * 15; // todo: extract constant
                            minAngle = a;
                        }
                    }
                }
            }

            if (aimVector != Vector3.Zero)
                aimVector.Normalize();

            return targetPlayer;
        }


        /// <summary>
        /// positions the player randomly on an island
        /// </summary>
        private void PositionOnRandomIsland()
        {
            int cnt = Game.Instance.Simulation.IslandManager.Count;
            Entity island = Game.Instance.Simulation.IslandManager[0];
            // try at most count*2 times
            int i = 0;
            for(; i < cnt*2; i++)
            {
                bool valid = true;
                int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                island = Game.Instance.Simulation.IslandManager[islandNo];

                // check no players on island
                if (island.GetInt("players_on_island") > 0)
                    valid = false;

                // for 2nd round (> cnt) we accept respawn on powerups
                if (i < cnt)
                {
                    // check island is high enough
                    if (island.GetVector3("position").Y < 80) // todo: extract constant
                    {
                        continue;
                    }

                    // check no powerup on island
                    foreach (Entity powerup in Game.Instance.Simulation.PowerupManager)
                    {
                        if (island.Name == powerup.GetString("island_reference"))
                        {
                            valid = false;
                            break;
                        }
                    }

                    // check island is far enough away from other players,
                    foreach (Entity p in Game.Instance.Simulation.PlayerManager)
                    {
                        Vector3 dist = island.GetVector3("position") - p.GetVector3("position");
                        dist.Y = 0; // ignore y component
                        if (dist.Length() < constants.GetFloat("respawn_min_distance_to_players"))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                    break; // ok
                else
                    continue; // select another
            }
            SetActiveIsland(island);
            player.SetVector3("position", GetLandingPosition(island) + Vector3.UnitY*500);
        }

        /// <summary>
        /// sets the activeisland
        /// </summary>
        private void SetActiveIsland(Entity island)
        {
            if (player.HasProperty("hud"))
            {
                player.GetProperty<HUDProperty>("hud").JetpackUsable = false;
            }
            if (player.HasProperty("render"))
            {
                if (won == true)
                {
                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "win";
                }
                else
                {
                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                }
            }

            if (simpleJumpIsland != null)
            {
                StopSimpleJump();
            }

            // register with active
            island.GetAttribute<Vector3Attribute>("position").ValueChanged += IslandPositionHandler;
            island.SetInt("players_on_island", island.GetInt("players_on_island") + 1);
            
            // set
            activeIsland = island;
            player.SetString("active_island", island.Name);
        }


        /// <summary>
        /// resets the activeisland
        /// </summary>
        private void LeaveActiveIsland()
        {
            if (activeIsland != null)
            {
                //Console.WriteLine(player.Name+" left island");
                if (destinationIsland == null && simpleJumpIsland == null)
                {
                    player.GetProperty<HUDProperty>("hud").JetpackUsable = true;
                }

                activeIsland.GetAttribute<Vector3Attribute>("position").ValueChanged -= IslandPositionHandler;
                activeIsland.SetInt("players_on_island", activeIsland.GetInt("players_on_island") - 1);

                activeIsland = null;
                player.SetString("active_island", "");

                // disable selection
                if (selectedIsland != null)
                {
                    RemoveSelectionArrow();
                }
            }
        }

        private void StopSimpleJump()
        {
            simpleJumpIsland.GetAttribute<Vector3Attribute>("position").ValueChanged -= IslandPositionHandler;
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
            player.SetString("jump_island", "");
            simpleJumpIsland = null;
        }

        public static Vector3 GetPosition(Entity player)
        {
            return player.GetVector3("position");
        }

        public static Vector3 GetScale(Entity player)
        {
            if (player.HasVector3("scale"))
            {
                return player.GetVector3("scale");
            }
            else
            {
                return Vector3.One;
            }
        }

        public static Quaternion GetRotation(Entity player)
        {
            if (player.HasQuaternion("rotation"))
            {
                return player.GetQuaternion("rotation");
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        class ControllerInput
        {
            private GamePadState oldGPState;
            private KeyboardState oldKBState;

            private GamePadState gamePadState;
            private KeyboardState keyboardState;

            public bool StartHitAnimation { set; get; }

            public void Update(PlayerIndex playerIndex, SimulationTime simTime)
            {
                gamePadState = GamePad.GetState(playerIndex);
                keyboardState = Keyboard.GetState(playerIndex);

                #region joysticks

                leftStickX = gamePadState.ThumbSticks.Left.X;
                leftStickY = -gamePadState.ThumbSticks.Left.Y;
                if (PlayerControllerProperty.LeftStickSelection)
                {
                    rightStickX = leftStickX;
                    rightStickY = leftStickY;
                }
                else
                {
                    rightStickX = gamePadState.ThumbSticks.Right.X;
                    rightStickY = -gamePadState.ThumbSticks.Right.Y;
                }

                flameStickX = gamePadState.ThumbSticks.Right.X;
                flameStickY = -gamePadState.ThumbSticks.Right.Y;

                dPadX = (gamePadState.DPad.Right == ButtonState.Pressed)? 1.0f : 0.0f
                    - ((gamePadState.DPad.Left == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadY = (gamePadState.DPad.Down == ButtonState.Pressed) ? 1.0f : 0.0f
                    - ((gamePadState.DPad.Up == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadPressed = dPadX != 0 || dPadY != 0;

                /*
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    leftStickX = gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.D))
                    {
                        leftStickX = -gamepadEmulationValue;
                    }

                if (keyboardState.IsKeyDown(Keys.W))
                {
                    leftStickY = gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.S))
                    {
                        leftStickY = -gamepadEmulationValue;
                    }

                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    rightStickX = gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        rightStickX = -gamepadEmulationValue;
                    }

                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    rightStickY = -gamepadEmulationValue;
                }
                else
                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        rightStickY = gamepadEmulationValue;
                    }
                */

                moveStickMoved = leftStickX > StickMovementEps || leftStickX < -StickMovementEps
                    || leftStickY > StickMovementEps || leftStickY < -StickMovementEps;
                rightStickMoved = rightStickX > StickMovementEps || rightStickX < -StickMovementEps
                    || rightStickY > StickMovementEps || rightStickY < -StickMovementEps;
                flameStickMoved = flameStickX > StickMovementEps || flameStickX < -StickMovementEps
                    || flameStickY > StickMovementEps || flameStickY < -StickMovementEps;

                #endregion

                #region action buttons

                SetStates(RepulsionButtons, JetpackKey, out repulsionButtonPressed, out repulsionButtonHold, out repulsionButtonReleased);
                SetStates(JetpackButtons, JetpackKey, out jetpackButtonPressed, out jetpackButtonHold, out jetpackButtonReleased);
                SetStates(IceSpikeButtons, IceSpikeKey, out iceSpikeButtonPressed, out iceSpikeButtonHold, out iceSpikeButtonReleased);
                SetStates(HitButtons, HitKey, out hitButtonPressed, out hitButtonHold, out hitButtonReleased);
                SetStates(FlamethrowerButtons, FlamethrowerKey, out flamethrowerButtonPressed, out flamethrowerButtonHold, out flamethrowerButtonReleased);
                SetStates(RunButtons, RunKey, out runButtonPressed, out runButtonHold, out runButtonReleased);

                #endregion

                if (hitButtonPressed)
                {
                    hitButtonPressedAt = simTime.At;
                    StartHitAnimation = true;
                }

                oldGPState = gamePadState;
                oldKBState = keyboardState;
            }

            private void SetStates(Buttons[] buttons, Keys key,
                out bool pressedIndicator,
                out bool holdIndicator,
                out bool releasedIndicator)
            {
                pressedIndicator = false;
                releasedIndicator = false;
                holdIndicator = false;

                for (int i = 0; i < buttons.Length; i++)
                {
                    pressedIndicator |= GetPressed(buttons[i]);
                    releasedIndicator |= GetReleased(buttons[i]);
                    holdIndicator |= GetHold(buttons[i]);
                }

                pressedIndicator |= GetPressed(key);
                releasedIndicator |= GetReleased(key);
                holdIndicator |= GetHold(key);
            }

            private bool GetPressed(Buttons button)
            {
                return gamePadState.IsButtonDown(button)
                    && oldGPState.IsButtonUp(button);
            }

            private bool GetReleased(Buttons button)
            {
                return gamePadState.IsButtonUp(button)
                    && oldGPState.IsButtonDown(button);
            }

            private bool GetHold(Buttons button)
            {
                return gamePadState.IsButtonDown(button)
                    && oldGPState.IsButtonDown(button);
            }

            private bool GetPressed(Keys key)
            {
                return keyboardState.IsKeyDown(key)
                    && oldKBState.IsKeyUp(key);
            }

            private bool GetReleased(Keys key)
            {
                return keyboardState.IsKeyUp(key)
                    && oldKBState.IsKeyDown(key);
            }

            private bool GetHold(Keys key)
            {
                return keyboardState.IsKeyDown(key)
                    && oldKBState.IsKeyDown(key);
            }

            public void Reset()
            {
                leftStickX = leftStickY = 0;
                rightStickX = rightStickY = 0;
                flameStickX = flameStickY = 0;
                dPadX = dPadY = 0;

                moveStickMoved = flameStickMoved = dPadPressed = false;
                runButtonPressed = repulsionButtonPressed = jetpackButtonPressed = flamethrowerButtonPressed = iceSpikeButtonPressed = hitButtonPressed = false;
                runButtonReleased = repulsionButtonReleased = jetpackButtonReleased = flamethrowerButtonReleased = iceSpikeButtonReleased = hitButtonReleased = false;
                runButtonHold = repulsionButtonHold = jetpackButtonHold = flamethrowerButtonHold = iceSpikeButtonHold = hitButtonHold = false;
            }

           
            // joystick
            public float leftStickX, leftStickY;
            public bool moveStickMoved;
            public float rightStickX, rightStickY;
            public bool rightStickMoved;
            public float flameStickX, flameStickY;
            public bool flameStickMoved;
            public bool dPadPressed;
            public float dPadX, dPadY;

            // buttons
            public bool runButtonPressed, repulsionButtonPressed, jetpackButtonPressed, flamethrowerButtonPressed, 
                iceSpikeButtonPressed, hitButtonPressed;
            public bool runButtonReleased, repulsionButtonReleased, jetpackButtonReleased, flamethrowerButtonReleased, 
                iceSpikeButtonReleased, hitButtonReleased;
            public bool runButtonHold, repulsionButtonHold, jetpackButtonHold, flamethrowerButtonHold, 
                iceSpikeButtonHold, hitButtonHold;

            // times
            public float hitButtonPressedAt = float.NegativeInfinity;

            private static float gamepadEmulationValue = -1f;
        }

        private readonly ControllerInput controllerInput = new ControllerInput();
    }


}

