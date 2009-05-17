using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;
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
        private static readonly bool DeselectOnRelease = false;
        public static readonly bool ImuneToIslandPush = true;
        #endregion

        #region button assignments
        // eps for registering move
        private static readonly float StickMovementEps = 0.1f;
        
        // gamepad buttons
        private static readonly Buttons[] RepulsionButtons = { Buttons.LeftTrigger };
        private static readonly Buttons[] AttractionButtons = { Buttons.A };
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
        private static readonly Keys AttractionKey = Keys.LeftControl;
        private static readonly Keys RunKey = Keys.LeftControl;
        #endregion


        private Entity player;
        private Entity constants;
        private LevelData templates;

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
        private Vector3 lastStickDir = Vector3.Zero;
        private Entity destinationIsland = null;
        private float destinationOrigDist = 0;
        private float destinationOrigY = 0;
        private Vector3 lastIslandDir = Vector3.Zero;
        private float islandJumpPerformedAt = 0;
        private Entity attractedIsland = null;
        private bool repulsionActive = false;

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

        public void OnAttached(Entity player)
        {
            player.Update += OnUpdate;

            this.player = player;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];
            this.templates = Game.Instance.ContentManager.Load<LevelData>("Level/DynamicTemplates");

            player.AddQuaternionAttribute("rotation", Quaternion.Identity);
            player.AddVector3Attribute("velocity", Vector3.Zero);

            player.AddVector3Attribute("collision_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("player_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddVector3Attribute("island_jump_velocity", Vector3.Zero);

            player.AddIntAttribute("energy", constants.GetInt("max_energy"));
            player.AddIntAttribute("health", constants.GetInt("max_health"));
            player.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            player.AddIntAttribute("jumps", 0);
            player.AddFloatAttribute("repulsion_seconds", 0);

            player.AddIntAttribute("kills", 0);
            player.AddIntAttribute("deaths", 0);

            player.AddIntAttribute("frozen", 0);
            player.AddStringAttribute("active_island", "");

            Game.Instance.Simulation.EntityManager.EntityRemoved += EntityRemovedHandler;
            ((CollisionProperty)player.GetProperty("collision")).OnContact += PlayerCollisionHandler;

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

        public void OnDetached(Entity player)
        {
            player.Update -= OnUpdate;

            if (arrow != null && Game.Instance.Simulation.EntityManager.ContainsEntity(arrow))
            {
                Game.Instance.Simulation.EntityManager.Remove(arrow);
            }

            if (flame != null && Game.Instance.Simulation.EntityManager.ContainsEntity(flame))
            {
                Game.Instance.Simulation.EntityManager.Remove(flame);
            }

            Game.Instance.Simulation.EntityManager.EntityRemoved -= EntityRemovedHandler;

            if (player.HasProperty("collision"))
            {
                ((CollisionProperty)player.GetProperty("collision")).OnContact -= PlayerCollisionHandler;
            }
        }

        private void OnUpdate(Entity player, SimulationTime simTime)
        {
            float dt = simTime.Dt;
            float at = simTime.At;

            if (CheckAndPerformDeath(player, at))
            {
                // don't execute any other code as long as player is dead
                return;
            }

            #region collision reaction
            // reset collision response
            if (simTime.At > collisionAt + 500) // todo: extract constant
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
            controllerInput.Update(playerIndex, simTime);

            #region movement

            // jetpack
            PerformJetpackMovement(simTime, dt, ref playerVelocity, ref fuel);

            // xz movement
            PerformStickMovement(player, dt, ref playerPosition, fuel);

            // perform island jump
            PerformIslandJump(player, dt, at, ref playerPosition, ref playerVelocity);

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

            PerformIceSpikeAction(player, at, playerPosition);

            PerformFlamethrowerAction(player, ref playerPosition);

            PerformIslandRepulsionAction(simTime, ref fuel);

            // island selection and attraction
            bool allowSelection = attractedIsland == null && destinationIsland == null;
            PerformIslandSelectionAction(at, allowSelection, ref playerPosition);
            PerformIslandJumpAction(ref playerPosition, ref playerVelocity);
//            PerformIslandAttractionAction(player, allowSelection, ref playerPosition, ref playerVelocity);
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
            if (playerPosition.Y < lava.GetVector3("position").Y)
                PlayerLavaCollisionHandler(simTime, player, lava);

            if(controllerInput.StartHitAnimation)
            {
                controllerInput.StartHitAnimation = false;
                (player.GetProperty("render") as RobotRenderProperty).NextOnceState = "hit";
            }

            Debug.Assert(!(selectedIsland == null) || !arrow.HasProperty("render"));
        }

        private void PerformIslandAttractionAction(Entity player, bool allowSelection,
            ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            // island attraction start
            if (controllerInput.attractionButtonPressed
                && selectedIsland != null
                && destinationIsland == null // not during jump
                && allowSelection)
            {
                attractedIsland = selectedIsland;
                attractedIsland.SetString("attracted_by", player.Name);
                ((StringAttribute)attractedIsland.GetAttribute("attracted_by")).ValueChanged += IslandAttracedByChangeHandler;
            }
            else
            // running attraction
            if (attractedIsland != null)
            {
                // deactivate if button released
                if(!controllerInput.attractionButtonHold
                    || destinationIsland != null) // if we initiated jump, abort attraction
                {
                    // start attraction stop (has timeout)
                    attractedIsland.SetBool("stop_attraction", true);

                    // initiate jump
                    if (selectedIslandInFreeJumpRange)
                    {
                        StartIslandJump(attractedIsland, ref playerPosition, ref playerVelocity);
                    }

                    // remove selection
                    RemoveSelectionArrow();
                }
            }
        }

        private void PerformIslandJumpAction(ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            if (controllerInput.jetpackButtonPressed
                && player.GetInt("frozen") <= 0)
            {
                // island jump start
                if (controllerInput.jetpackButtonPressed
                    && selectedIsland != null
                    && destinationIsland == null
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

                }
            }
        }

        private void StartIslandJump(Entity island, ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            destinationIsland = island;

            ((Vector3Attribute)island.Attributes["position"]).ValueChanged += IslandPositionHandler;


            // calculate time to travel to island (in xz plane) using an iterative approach
            Vector3 islandDir = GetLandingPosition(destinationIsland) - playerPosition;
            islandDir.Y = 0;
            lastIslandDir = islandDir;
            destinationOrigDist = islandDir.Length();
            destinationOrigY = playerPosition.Y;

            LeaveActiveIsland();
        }

        private void PerformIslandSelectionAction(float at, bool allowSelection, ref Vector3 playerPosition)
        {
            if (allowSelection)
            {
                if (/*controllerInput.rightStickMoved
                    &&*/ activeIsland != null) // must be standing on island
                {
                    //    Vector3 selectionDirection = new Vector3(controllerInput.rightStickX, 0, controllerInput.rightStickY);
                  //  stickDir.Normalize();
                    Vector3 selectionDirection = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                    // only allow reselection if stick moved slightly
                    bool stickMoved = Vector3.Dot(lastStickDir, selectionDirection) < constants.GetFloat("island_reselection_max_value");
                    if (/*(selectedIsland == null || stickMoved)
                        && */ at > islandSelectedAt + constants.GetFloat("island_reselection_timeout"))
                    {
                        //                        Console.WriteLine("new selection (old: " + ((selectedIsland != null) ? selectedIsland.Name : "") + "): " + lastStickDir + "." + stickDir + " = " +
                        //                            Vector3.Dot(lastStickDir, stickDir));

                        // select closest island in direction of stick
                        lastStickDir = selectionDirection;
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
                    if (selectedIsland != null
                        && at > islandSelectedAt + constants.GetFloat("island_deselection_timeout")
                        /*&& DeselectOnRelease*/)
                    {
                        RemoveSelectionArrow();
                    }
                }
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
                    (arrow.GetProperty("render") as ArrowRenderProperty).JumpPossible = false;//SquashParams = new Vector2(1000f, 0.8f);
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
            lastStickDir = Vector3.Zero;
            islandSelectedAt = 0;
        }

        private void PerformIslandRepulsionAction(SimulationTime simTime, ref int fuel)
        {
            if (activeIsland != null)
            {
                // island repulsion start
                if (controllerInput.repulsionButtonPressed
                    && player.GetInt("energy") > constants.GetInt("island_repulsion_start_min_energy"))
                {
                    Vector3 velocity = new Vector3(controllerInput.leftStickX, 0, controllerInput.leftStickY);
                    Vector3 currentVelocity = activeIsland.GetVector3("repulsion_velocity");
                    velocity *= constants.GetFloat("island_repulsion_start_velocity_multiplier");
                    activeIsland.SetVector3("repulsion_velocity", currentVelocity + velocity);

                    player.SetInt("energy", player.GetInt("energy") - constants.GetInt("island_repulsion_start_energy_cost"));

                    repulsionActive = true;
                }
                else
                    // island repulsion
                    if (repulsionActive
                        && controllerInput.repulsionButtonHold
                        //                && player.GetFloat("repulsion_seconds") > 0
                        && player.GetInt("energy") > 0
                        )
                    {
                        Vector3 velocity = new Vector3(controllerInput.leftStickX, 0, controllerInput.leftStickY);
                        Vector3 currentVelocity = activeIsland.GetVector3("repulsion_velocity");
                        velocity *= constants.GetFloat("island_repulsion_velocity_multiplier");
                        activeIsland.SetVector3("repulsion_velocity", currentVelocity + velocity);

                        Game.Instance.Simulation.ApplyPerSecondSubstraction(player, "repulsion_energy_cost",
                            constants.GetInt("island_repulsion_energy_cost_per_second"), player.GetIntAttribute("energy"));

                        if (player.GetInt("energy") <= 0)
                        {
                            repulsionActive = false;
                        }
                    }
            }
        }

        private void PerformFlamethrowerAction(Entity player, ref Vector3 playerPosition)
        {
            // flamethrower
            if (((controllerInput.flamethrowerButtonPressed
                || controllerInput.flamethrowerButtonHold)
                && !RightStickFlame)
                || (controllerInput.flameStickMoved && RightStickFlame)
                && activeIsland != null) // only allowed on ground
            {
                if (flame == null)
                {
                    if (player.GetInt("energy") > constants.GetInt("flamethrower_warmup_energy_cost"))
                    {
                        // indicate 
                        flameThrowerSoundInstance = flameThrowerSound.Play(Game.Instance.EffectsVolume, 1, 0, true);

                        // todo: extract offset
                        Vector3 pos = new Vector3(playerPosition.X + 10, playerPosition.Y + 10, playerPosition.Z);
                        Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                        flame = new Entity("flame" + "_" + player.Name);
                        flame.AddStringAttribute("player", player.Name);

                        flame.AddVector3Attribute("velocity", viewVector);
                        flame.AddVector3Attribute("position", pos);
                        flame.AddQuaternionAttribute("rotation", GetRotation(player));

                        Game.Instance.Simulation.EntityManager.AddDeferred(flame, "flamethrower_base", templates);

                        // indicate on model
                        (player.GetProperty("render") as RobotRenderProperty).NextPermanentState = "attack_long";
                    }
                }
                else
                {
                    // only rotate when fueled
                    if (flame.GetBool("fueled"))
                    {
                        // change y rotation towards player in range
                        Vector3 aimVector;
                        Vector3 direction;
                        if (RightStickFlame)
                            direction = new Vector3(controllerInput.flameStickX, 0, controllerInput.flameStickY);
                        else
                            direction = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));
                        direction.Normalize();
                        Entity targetPlayer = SelectBestPlayerInDirection(ref playerPosition, ref direction, 
                            constants.GetFloat("flamethrower_aim_angle"), out aimVector);
                        if (targetPlayer != null)
                        {
                            // we only aim perfectly in y, but give a little support in x/z
                            aimVector.X = direction.X * 0.5f + aimVector.X * 0.5f;
                            aimVector.Z = direction.Z * 0.5f + direction.Z * 0.5f;
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
                            (player.GetProperty("render") as RobotRenderProperty).NextPermanentState = "idle";
                            flame.SetBool("fueled", false);
                        }
                    }
                }
            }
            else
            {
                if (flame != null)
                {
                    (player.GetProperty("render") as RobotRenderProperty).NextPermanentState = "idle";
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
                        (player.GetProperty("render") as RobotRenderProperty).NextPermanentState = "walk";

                        // on island ground
                        if (controllerInput.runButtonHold)
                        {
                            playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_run_multiplier");
                            playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_run_multiplier");
                        }
                        else
                        {
                            playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_walk_multiplier");
                            playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_walk_multiplier");
                        }

                        // check position a bit further in walking direction to be still on island
                        Vector3 checkPos = playerPosition + new Vector3(controllerInput.leftStickX * 20,
                            0, controllerInput.leftStickY * 20);

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
                if((player.GetProperty("render") as RobotRenderProperty).NextPermanentState != "idle")
                {
                    (player.GetProperty("render") as RobotRenderProperty).NextPermanentState = "idle";
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
                    if (player.GetVector3("hit_pushback_velocity") == Vector3.Zero)
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
                    playerVelocity += constants.GetVector3("gravity_acceleration") * dt;
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

                    (player.GetProperty("render") as BasicRenderProperty).Squash();
                    (destinationIsland.GetProperty("render") as IslandRenderProperty).Squash();

                    playerPosition = isectPt;

                    ((Vector3Attribute)destinationIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;

                    destinationIsland = null;
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
                    player.SetVector3("island_jump_velocity", Vector3.Normalize(islandDir)
                        /** constants.GetFloat("island_jump_speed")*/ * speed);
                }
            }
        }

        private void PerformJetpackMovement(SimulationTime simTime, float dt, ref Vector3 playerVelocity, ref int fuel)
        {
            if ((controllerInput.rightStickPressed
                || controllerInput.jetpackButtonHold)
                && activeIsland == null // only in air
                && destinationIsland == null // not while jump
                && flame == null // not in combination with flame
                && fuel > 0 // we need fuel
                && player.GetInt("frozen") <= 0 // jetpack doesn't work when frozen
            )
            {
                LeaveActiveIsland();

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
                    destinationIsland = null;
                    LeaveActiveIsland();

                    Game.Instance.ContentManager.Load<SoundEffect>("Sounds/death").Play(Game.Instance.EffectsVolume);
                    player.SetInt("deaths", player.GetInt("deaths") + 1);
                    player.SetInt("lives", player.GetInt("lives") - 1);

                    ((CollisionProperty)player.GetProperty("collision")).OnContact -= PlayerCollisionHandler;

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

                    deathExplosion.AddVector3Attribute("position", player.GetVector3("position"));

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
                        // activate
                        player.AddProperty("collision", new CollisionProperty());
                        player.AddProperty("render", new RobotRenderProperty());
                        player.AddProperty("shadow_cast", new ShadowCastProperty());
                        ((CollisionProperty)player.GetProperty("collision")).OnContact += PlayerCollisionHandler;

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
            Vector3 pos = island.GetVector3("position") + island.GetVector3("landing_offset");
            return pos;
        }

        private void PlayerCollisionHandler(SimulationTime simTime, Contact contact)
        {
            if (contact.EntityB.HasAttribute("kind"))
            {
                // slide on jump
                if (destinationIsland != null
                    && (contact.EntityB.GetString("kind") == "pillar"
                    || contact.EntityB.GetString("kind") == "island"))
                {
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
                // don't do any collision reaction with island we are standing on
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
                || island == attractedIsland)
            {
//                Console.WriteLine(player.Name + " collidet with " + island.Name);

                if (destinationIsland != null) // if we are in jump, don't active island
                {
                    return;
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
                    Vector3 velocity = -normal * 100; // todo: extract constant
                    player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
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
            Vector3 normal = pillar.GetVector3("position") - player.GetVector3("position");
            normal.Y = 0;
            if(normal != Vector3.Zero)
                normal.Normalize();
            // todo: extract constant
            Vector3 velocity = -normal * 1000;
            player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
        }

        private void PlayerCaveCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            // only collide with cave when in air
            if (activeIsland != null)
            {
                Vector3 pos = player.GetVector3("position");
                // todo: extract constants
                Vector3 velocity = (-Vector3.Normalize(pos) - Vector3.UnitY / 10) * 2000;
                velocity.Y = 0;
                player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
            }
        }

        private void PlayerLavaCollisionHandler(SimulationTime simTime, Entity player, Entity lava)
        {
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
                Vector3 velocity = normal * constants.GetVector3("hit_pushback_velocity_multiplier");
                otherPlayer.SetVector3("hit_pushback_velocity", velocity);
                otherPlayer.SetVector3("position", otherPlayer.GetVector3("position") + velocity * simTime.Dt);
                hitPerformedAt = simTime.At;

                // indicate in model
                (otherPlayer.GetProperty("render") as RobotRenderProperty).NextOnceState = "pushback";

                // leave island
                LeaveActiveIsland();
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

        private void IslandAttracedByChangeHandler(StringAttribute sender, String oldValue, String newValue)
        {
            if (oldValue != "" && newValue == "")
            {
                // remove handler
                sender.ValueChanged -= IslandAttracedByChangeHandler;

                // remove arrow
                if (arrow.HasProperty("render"))
                {
                    RemoveSelectionArrow();
                }

                // reset attraction and selection
                attractedIsland = null;
            }
        }

        private void EntityRemovedHandler(EntityManager manager, Entity entity)
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

                // disable attraction
                if (attractedIsland != null)
                {
                    attractedIsland.SetString("attracted_by", "");
                    attractedIsland = null;
                }
            }
            if (entity == attractedIsland)
            {
                attractedIsland = null;
            }

            // check if player was removed (lost all his lives)
            if(entity.HasAttribute("kind")
                && entity.GetString("kind") == "player")
            {
                // check if we are last man standing
                if (Game.Instance.Simulation.PlayerManager.Count == 1)
                {
                    won = true;
                    Game.Instance.Simulation.Phase = SimulationPhase.Outro;
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
                islandDir.Y = 0;
                float dist = islandDir.Length();
                float angle = (float)(Math.Acos(Vector3.Dot(dir, islandDir) / dist) / Math.PI * 180);
                if (dist < constants.GetFloat("island_jump_free_range"))
                {
                    // hack hack
                    island.SetBool("interactable", true);

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
                else
                {
                    // hack hack
                    island.SetBool("interactable", false);
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
            for(int i = 0; i < cnt*2; i++)
            {
                bool valid = true;
                int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                island = Game.Instance.Simulation.IslandManager[islandNo];

                // check no players on island
                if (island.GetInt("players_on_island") > 0)
                    valid = false;
                
                // check no powerup on island
                foreach(Entity powerup in Game.Instance.Simulation.PowerupManager)
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

                if (valid)
                    break; // ok
                else
                    continue; // select another
            }
            SetActiveIsland(island);
            player.SetVector3("position", GetLandingPosition(island));
        }

        /// <summary>
        /// sets the activeisland
        /// </summary>
        private void SetActiveIsland(Entity island)
        {
            //Console.WriteLine(player.Name + " activated island");

            // register with active
            ((Vector3Attribute)island.Attributes["position"]).ValueChanged += IslandPositionHandler;
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

                ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;
                activeIsland.SetInt("players_on_island", activeIsland.GetInt("players_on_island") - 1);

                activeIsland = null;
                player.SetString("active_island", "");

                // disable attraction
                if (attractedIsland != null)
                {
                    attractedIsland.SetString("attracted_by", "");
                    attractedIsland = null;
                }

                // disable selection
                if (selectedIsland != null)
                    RemoveSelectionArrow();
            }
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
                rightStickPressed = gamePadState.Buttons.RightStick == ButtonState.Pressed;

                flameStickX = gamePadState.ThumbSticks.Right.X;
                flameStickY = -gamePadState.ThumbSticks.Right.Y;

                dPadX = (gamePadState.DPad.Right == ButtonState.Pressed)? 1.0f : 0.0f
                    - ((gamePadState.DPad.Left == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadY = (gamePadState.DPad.Down == ButtonState.Pressed) ? 1.0f : 0.0f
                    - ((gamePadState.DPad.Up == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadPressed = dPadX != 0 || dPadY != 0;

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
                SetStates(AttractionButtons, AttractionKey, out attractionButtonPressed, out attractionButtonHold, out attractionButtonReleased);
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
           
            // joystick
            public float leftStickX, leftStickY;
            public bool moveStickMoved;
            public float rightStickX, rightStickY;
            public bool rightStickMoved, rightStickPressed;
            public float flameStickX, flameStickY;
            public bool flameStickMoved;
            public bool dPadPressed;
            public float dPadX, dPadY;

            // buttons
            public bool runButtonPressed, repulsionButtonPressed, jetpackButtonPressed, flamethrowerButtonPressed, 
                iceSpikeButtonPressed, hitButtonPressed, attractionButtonPressed;
            public bool runButtonReleased, repulsionButtonReleased, jetpackButtonReleased, flamethrowerButtonReleased, 
                iceSpikeButtonReleased, hitButtonReleased, attractionButtonReleased;
            public bool runButtonHold, repulsionButtonHold, jetpackButtonHold, flamethrowerButtonHold, 
                iceSpikeButtonHold, hitButtonHold, attractionButtonHold;

            // times
            public float hitButtonPressedAt = float.NegativeInfinity;

            private static float gamepadEmulationValue = -1f;
        }

        private readonly ControllerInput controllerInput = new ControllerInput();
    }


}

