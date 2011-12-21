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
    public class PlayerControllerProperty : PlayerBaseProperty
    {
        #region flags
        public static readonly bool RightStickFlame = false;
        public static readonly bool LeftStickSelection = true;
        public static readonly bool ImuneToIslandPush = true;
        #endregion

        private bool won = false;

        private Entity deathExplosion = null;

        private bool jetpackActive = false;

        private int iceSpikeCount = 0;
        private int iceSpikeRemovedCount = 0;
        private float iceSpikeFiredAt = 0;

        private Vector3 inwardsPushVelocity = Vector3.Zero;
        private float inwardsPushStartedAt = float.MaxValue;
        private bool canFallFromIsland = false;
        private double hitPerformedAt = 0;

        private float islandSelectedAt = 0;
        private Entity selectedIsland = null;
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

        private SoundEffectInstance jetpackSoundInstance;
        private SoundEffectInstance flameThrowerSoundInstance;

        // values which get reset on each update
        private float collisionAt = float.MinValue;
        private float movedAt = float.MinValue;

        public PlayerControllerProperty()
        {
        }

        public override void OnAttached(AbstractEntity player)
        {
            base.OnAttached(player);

            player.AddFloatAttribute("vibrateStartetAt", 0);

            player.AddQuaternionAttribute(CommonNames.Rotation, Quaternion.Identity);
            player.AddVector3Attribute(CommonNames.Velocity, Vector3.Zero);

            player.AddVector3Attribute("collision_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddVector3Attribute("island_jump_velocity", Vector3.Zero);

            player.AddFloatAttribute(CommonNames.Energy, constants.GetFloat(CommonNames.MaxEnergy));
            player.AddFloatAttribute(CommonNames.Health, constants.GetFloat(CommonNames.MaxHealth));
            player.GetAttribute<FloatAttribute>(CommonNames.Health).ValueChanged += HealthChangeHandler;

            player.AddIntAttribute("jumps", 0);
            player.AddFloatAttribute("repulsion_seconds", 0);

            player.AddIntAttribute("kills", 0);
            player.AddIntAttribute("deaths", 0);

            player.AddIntAttribute(CommonNames.Frozen, 0);
            player.AddStringAttribute("active_island", "");
            player.AddStringAttribute("jump_island", "");

            Game.Instance.Simulation.EntityManager.EntityRemoved += EntityRemovedHandler;
            player.GetProperty<CollisionProperty>("collision").OnContact += PlayerCollisionHandler;

            arrow = new Entity("arrow_" + player.Name);
            arrow.AddStringAttribute("player", player.Name);

            arrow.AddVector3Attribute(CommonNames.Color1, player.GetVector3("color1"));
            arrow.AddVector3Attribute(CommonNames.Color2, player.GetVector3("color2"));

            Game.Instance.Simulation.EntityManager.Add(arrow, "arrow_base", templates);

            player.SetVector3(CommonNames.PreviousPosition, player.GetVector3(CommonNames.Position));
        }

        public override void OnDetached(AbstractEntity player)
        {
            base.OnDetached(player);

            if (arrow != null && Game.Instance.Simulation.EntityManager.ContainsEntity(arrow))
            {
                Game.Instance.Simulation.EntityManager.Remove(arrow);
            }

            if (flame != null && Game.Instance.Simulation.EntityManager.ContainsEntity(flame))
            {
                Game.Instance.Simulation.EntityManager.Remove(flame);
            }

            if (flameThrowerSoundInstance != null)
                Game.Instance.AudioPlayer.Stop(flameThrowerSoundInstance);

            Game.Instance.Simulation.EntityManager.EntityRemoved -= EntityRemovedHandler;

            player.GetAttribute<FloatAttribute>(CommonNames.Health).ValueChanged -= HealthChangeHandler;

            if (player.HasProperty("collision"))
            {
                player.GetProperty<CollisionProperty>("collision").OnContact -= PlayerCollisionHandler;
            }
        }


        protected override void OnPlaying(Entity player, SimulationTime simTime)
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
            if (!HadCollision(simTime))
            {
                player.SetVector3("collision_pushback_velocity", Vector3.Zero);
            }
            if (simTime.At > inwardsPushStartedAt + 100) 
            {
                inwardsPushVelocity = Vector3.Zero;
            }
            #endregion

            Vector3 playerPosition = player.GetVector3(CommonNames.Position);
            Vector3 playerVelocity = player.GetVector3(CommonNames.Velocity);
            Vector3 collisionPushbackVelocity = player.GetVector3("collision_pushback_velocity");
            Vector3 hitPushbackVelocity = player.GetVector3("hit_pushback_velocity");
            
            // hack hack, should be set when hit
            if (hitPushbackVelocity.Length() > 0)
            {
                // when hit we can fall!
                canFallFromIsland = true;
            }

            if (collisionPushbackVelocity.Length() > 200)
            {
                collisionPushbackVelocity.Normalize();
                collisionPushbackVelocity *= 200;
            }


            // reset some stuff
            player.SetVector3(CommonNames.PreviousPosition, player.GetVector3(CommonNames.Position));

            #region movement

            // jetpack
            PerformJetpackMovement(simTime, dt, ref playerVelocity);

            // xz movement
            PerformStickMovement(player, dt, at, ref playerPosition);

            // perform island jump
            PerformIslandJump(player, dt, at, ref playerPosition, ref playerVelocity);

            // perform simple jump on self
            PerformSimpleJump(ref playerPosition);

            // only apply velocity if not on island
            ApplyGravity(dt, ref playerPosition, ref playerVelocity);

            // apply current velocity
            playerPosition += playerVelocity * dt;

            // pushback
            Simulation.ApplyPushback(ref playerPosition, ref collisionPushbackVelocity, 500f);
            Simulation.ApplyPushback(ref playerPosition, ref inwardsPushVelocity, 0f /*constants.GetFloat("player_pushback_deacceleration")*/);
            Simulation.ApplyPushback(ref playerPosition, ref hitPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"), 
                HitPushbackEndedHandler);

            PerformIslandPositioning(simTime, ref playerPosition);

            #endregion

            #region actions

            if (Game.Instance.Simulation.Phase == SimulationPhase.Game)
            {
                PerformIceSpikeAction(player, at, playerPosition);
                PerformFlamethrowerAction(player, ref playerPosition);
                PerformIslandRepulsionAction(simTime);
                PerformIslandSelectionAction(at, ref playerPosition);
                PerformIslandJumpAction(ref playerPosition, ref playerVelocity);
            }
            #endregion

            #region recharge
            // recharge energy

            player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) + simTime.Dt * constants.GetFloat("energy_recharge_per_second"));

            // count dow
            if (player.GetInt(CommonNames.Frozen) > 0)
            {
                player.SetInt(CommonNames.Frozen, player.GetInt(CommonNames.Frozen) - (int)simTime.DtMs);
                if (player.GetInt(CommonNames.Frozen) < 0)
                {
                    player.SetInt(CommonNames.Frozen, 0);
                }
            }
            #endregion

            player.SetVector3(CommonNames.Position, playerPosition);
            player.SetVector3(CommonNames.Velocity, playerVelocity);
            player.SetVector3("collision_pushback_velocity", collisionPushbackVelocity);
            player.SetVector3("hit_pushback_velocity", hitPushbackVelocity);

            // check collision with lava
            if (playerPosition.Y <= 0) // todo: extract constant?
            {
                Entity lava = Game.Instance.Simulation.EntityManager["lava"];
                PlayerLavaCollisionHandler(simTime, player, lava);
            }

            if(controllerInput.hitButtonPressed)
            {
                player.GetProperty<RobotRenderProperty>("render").NextOnceState = "hit";
                Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.MeleeNotHit);
            }

            Debug.Assert(!(selectedIsland == null) || !arrow.HasProperty("render"));
        }

        private bool HadCollision(SimulationTime simTime)
        {
            return simTime.At < collisionAt + 100;
        }

        private void PerformIslandPositioning(SimulationTime simTime, ref Vector3 playerPosition)
        {
            if (activeIsland != null)
            {
                // position on island surface
                Vector3 isectPt = Vector3.Zero;
                if (Simulation.GetPositionOnSurface(ref playerPosition, activeIsland, out isectPt))
                {
                    Vector3 previousPosition = player.GetVector3(CommonNames.PreviousPosition);
                    // check movement no to high
                    if (previousPosition.Y < isectPt.Y
                        && isectPt.Y - previousPosition.Y > 20
                        && activeIsland.GetBool("allow_props_collision"))
                    {
                        playerPosition = previousPosition;
                        inwardsPushVelocity = Vector3.Zero;
                    }
                    else
                    {
                        // set position to contact point
                        playerPosition.Y = isectPt.Y + 1; // todo: make constant?
                    }
                }
                else
                    // not over island anymore
                    if (canFallFromIsland)
                    {
                        LeaveActiveIsland();
                        // skip check below
                        return; 
                    }
                    else
                        // only reset position if not already being pushed inwards
                        if(inwardsPushVelocity == Vector3.Zero)
                        {
                            playerPosition = player.GetVector3(CommonNames.PreviousPosition);
                        }

                if(!canFallFromIsland)
                {
                    // check if we are to close to island border
                    float islandNonWalkingRangeMultiplier = constants.GetFloat("island_non_walking_range_multiplier")
                        * activeIsland.GetVector3(CommonNames.Scale).X / 100; // normalized to scale of 100
                    Vector3 islandDir = (activeIsland.GetVector3(CommonNames.Position) - playerPosition);
                    islandDir.Y = 0;
                    if (islandDir != Vector3.Zero)
                    {
                        islandDir.Normalize();
                        Vector3 checkPos = playerPosition;
                        checkPos.X -= islandDir.X * islandNonWalkingRangeMultiplier;
                        checkPos.Z -= islandDir.Z * islandNonWalkingRangeMultiplier;

                        // if checkpoint is outside of island push inwards
                        if (!Simulation.GetPositionOnSurface(ref checkPos, activeIsland, out isectPt))
                        {
                            Vector3 velocity = islandDir;
                            velocity.X *= constants.GetFloat("island_non_walking_inwards_acceleration") * simTime.Dt;
                            velocity.Z *= constants.GetFloat("island_non_walking_inwards_acceleration") * simTime.Dt;
                            inwardsPushVelocity += velocity;
                            inwardsPushStartedAt = Game.Instance.Simulation.Time.At;
                        }
                        else
                        // abort current inwards movement
                        {
                            inwardsPushVelocity = Vector3.Zero;
                        }
                    }
                }
            }
        }


        private void PerformSimpleJump(ref Vector3 playerPosition)
        {
            Vector3 previousPosition = player.GetVector3(CommonNames.PreviousPosition);
            if (simpleJumpIsland != null)
            {
                // check position a bit further in walking direction to be still on island
                float islandNonWalkingRangeMultiplier = constants.GetFloat("island_non_walking_range_multiplier")
                    * simpleJumpIsland.GetVector3(CommonNames.Scale).X / 100; // normalized to scale of 100
                Vector3 checkPos = playerPosition + new Vector3(controllerInput.leftStickX * islandNonWalkingRangeMultiplier,
                    0, controllerInput.leftStickY * islandNonWalkingRangeMultiplier); // todo: extract constant

                Vector3 isectPt;
                if (!Simulation.GetPositionOnSurface(ref checkPos, simpleJumpIsland, out isectPt))
                {
                    // prohibit movement
                    playerPosition.X = previousPosition.X;
                    playerPosition.Z = previousPosition.Z;
                }
                else
                {
                    // check movement no to high
                    if (previousPosition.Y < isectPt.Y
                        && isectPt.Y - previousPosition.Y > 20
                        && simpleJumpIsland.GetBool("allow_props_collision"))
                    {
                        playerPosition.X = previousPosition.X;
                        playerPosition.Z = previousPosition.Z;
                    }
                }
            }
        }

        private void PerformIslandJumpAction(ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            if (controllerInput.jumpButtonPressed
                && !repulsionActive // no jumps during repulsion
                && activeIsland != null // and only when standing on island.. (obviously)
                && player.GetInt(CommonNames.Frozen) <= 0) // and not when frozen
            {
                // island jump start
                if (selectedIsland != null)
                {
                    if (Game.Instance.Simulation.Time.At > islandJumpPerformedAt + 200) // don't jump all the time
                    {
                        StartIslandJump(selectedIsland, ref playerPosition, ref playerVelocity);
                    }
                }
                else // only jump up
                {
                    StartSimpleJump(ref playerVelocity);
                }
            }
        }

        private void StartSimpleJump(ref Vector3 playerVelocity)
        {
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.JumpStart);

            simpleJumpIsland = activeIsland;

            LeaveActiveIsland();

            // set attribute
            player.SetString("jump_island", simpleJumpIsland.Name);

            // ensure we trak island movement
            simpleJumpIsland.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged += IslandPositionHandler;

            // initiate jump
            playerVelocity = (float)Math.Sqrt(constants.GetFloat("simple_jump_height") / constants.GetVector3("gravity_acceleration").Length())
                * -constants.GetVector3("simple_jump_gravity_acceleration");

            // and adapt model
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "jump";
        }

        private void StartIslandJump(Entity island, ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.JumpStart);

            destinationIsland = island;

            LeaveActiveIsland();

            // calculate time to travel to island (in xz plane) using an iterative approach
            Vector3 islandDir = GetLandingPosition(destinationIsland) - playerPosition;
            islandDir.Y = 0;
            lastIslandDir = islandDir;
            destinationOrigDist = islandDir.Length();
            destinationOrigY = playerPosition.Y;

            island.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged += IslandPositionHandler;

            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "jump";
        }

        private void PerformIslandSelectionAction(float at, ref Vector3 playerPosition)
        {
            if (activeIsland != null) 
            {
                if (player.GetInt(CommonNames.Frozen) <= 0) // when we are frozen we cannot jump -> no selection
                {
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
                                arrow.AddProperty("render", new ArrowRenderProperty(), true);
                                //arrow.AddProperty("shadow_cast", new ShadowCastProperty());
                            }
                        }
                    }
                }

                if ((selectedIsland != null
                && at > islandSelectedAt + constants.GetFloat("island_deselection_timeout")))
                {
                    RemoveSelectionArrow();
                }
            }
            else
            {
                // immediately remove selection if island not active anymore
                if (selectedIsland != null)
                    RemoveSelectionArrow();
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

        private void PerformIslandRepulsionAction(SimulationTime simTime)
        {
            if (activeIsland != null)
            {
                // island repulsion start
                if (!repulsionActive
                    && (controllerInput.repulsionButtonPressed
                    || controllerInput.repulsionButtonHold)
                    && activeIsland.GetString("repulsed_by") == ""
                    && player.GetFloat(CommonNames.Energy) > constants.GetInt("island_repulsion_start_min_energy"))
                {
                    if (controllerInput.moveStickMoved)
                    {
                        islandRepulsionStartDir = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));
                        islandRepulsionLastStickDir = islandRepulsionStartDir;
                        islandRepulsionStartRotation = activeIsland.GetQuaternion(CommonNames.Rotation);

                        Vector3 currentVelocity = activeIsland.GetVector3("repulsion_velocity");
                        Vector3 velocity = islandRepulsionStartDir *
                            constants.GetFloat("island_repulsion_start_velocity_multiplier");
                        activeIsland.SetVector3("repulsion_velocity", currentVelocity + velocity);

                        player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - constants.GetInt("island_repulsion_start_energy_cost"));

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
                        && player.GetFloat(CommonNames.Energy) > 0
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
                            activeIsland.SetQuaternion(CommonNames.Rotation, Quaternion.CreateFromRotationMatrix(rotationStart * rotationMatrix));
                        }
                        */


                        Vector3 currentVelocity = activeIsland.GetVector3("repulsion_velocity");
                        activeIsland.SetVector3("repulsion_velocity", currentVelocity +
                            dir * constants.GetFloat("island_repulsion_acceleration") * simTime.Dt);

                        player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - simTime.Dt * constants.GetFloat("island_repulsion_energy_cost_per_second"));

                        if (player.GetFloat(CommonNames.Energy) <= 0)
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
                    StopRepulsion();
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
            if (player.HasProperty("hud"))
            {
                player.GetProperty<HUDProperty>("hud").RepulsionUsable = false;
            }
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
                && player.GetInt(CommonNames.Frozen) <= 0 // not allowed when frozen
                && activeIsland != null) // only allowed on ground
            {
                if (flame == null)
                {
                    if (player.GetFloat(CommonNames.Energy) > constants.GetInt("flamethrower_warmup_energy_cost"))
                    {
                        // indicate 
                        flameThrowerSoundInstance = Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.FlameThrowerLoop, true);

                        Vector3 pos = playerPosition + constants.GetVector3("flamethrower_offset");
                        Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                        flame = new Entity("flame" + "_" + player.Name);
                        flame.AddStringAttribute("player", player.Name);

                        flame.AddVector3Attribute(CommonNames.Velocity, viewVector);
                        flame.AddVector3Attribute(CommonNames.Position, pos);
                        flame.AddQuaternionAttribute(CommonNames.Rotation, GetRotation(player));

                        Game.Instance.Simulation.EntityManager.AddDeferred(flame, "flamethrower_base", templates);

                        // indicate on model
                        player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "attack_long";
                    }
                }
                else
                {
                    // only rotate when fueled
                    if (flame.GetBool(CommonNames.Fueled))
                    {
                        // change y rotation towards player in range
                        Vector3 aimVector;
                        Vector3 offsetDir = Vector3.Normalize(constants.GetVector3("flamethrower_offset"));
                        Vector3 flamePos = flame.GetVector3(CommonNames.Position);
                        Vector3 direction = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));
                        float maxAngle = constants.GetFloat("flamethrower_aim_angle");
                        Entity targetPlayer = SelectBestPlayerInDirection(ref playerPosition, ref direction, maxAngle, out aimVector);
                        if (targetPlayer != null)
                        {
                            // get angle between stick dir and aimVector 
                            Vector3 aimVectorXZ = aimVector;
                            aimVectorXZ.Y = 0;
                            float a = (float)(Math.Acos(Vector3.Dot(aimVectorXZ, direction)) / Math.PI * 180);
                            float dirM = a / maxAngle / 2;
                            float aimM = 1 - dirM;

                            // correct target vector based on flame positon
                            Vector3 targetPos = targetPlayer.GetVector3(CommonNames.Position) + Vector3.UnitY * targetPlayer.GetVector3(CommonNames.Scale).Length() * 2; // todo: extract constant
                            aimVector = targetPos - flamePos;
                            if (aimVector != Vector3.Zero)
                                aimVector.Normalize();

                            // correct direciton based on flame position
                            direction = direction - offsetDir;
                            if (direction != Vector3.Zero)
                                direction.Normalize();

//                            Console.WriteLine("a: "+a+"°; max: "+maxAngle+"°; aimM: " + aimM + "; dirM: " + dirM);

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

                        flame.SetQuaternion(CommonNames.Rotation, targetQ);

                        // stop when energy runs out
                        if (player.GetFloat(CommonNames.Energy) <= 0)
                        {
                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                            flame.SetBool(CommonNames.Fueled, false);
                        }
                    }
                }
            }
            else
            {
                if (flame != null)
                {
                    player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                    flame.SetBool(CommonNames.Fueled, false);
                }
            }
        }

        private void PerformIceSpikeAction(Entity player, float at, Vector3 playerPosition)
        {
            // ice spike
            if (controllerInput.iceSpikeButtonPressed && player.GetFloat(CommonNames.Energy) > constants.GetInt("ice_spike_energy_cost")
                && at > iceSpikeFiredAt + constants.GetInt("ice_spike_cooldown"))
            {
                // indicate 
                Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.IceSpikeFire);

                Vector3 pos = new Vector3(playerPosition.X, playerPosition.Y + player.GetVector3(CommonNames.Scale).Y, playerPosition.Z);

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

                iceSpike.AddVector3Attribute(CommonNames.Velocity, initVelocity);
                iceSpike.AddVector3Attribute(CommonNames.Position, pos);

                iceSpike.AddStringAttribute("bv_type", "sphere");

                Game.Instance.Simulation.EntityManager.AddDeferred(iceSpike, "ice_spike_base", templates);

                // update states
                player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - constants.GetInt("ice_spike_energy_cost"));
                iceSpikeFiredAt = at;
            }
        }

        private void PerformStickMovement(Entity player, float dt, float at, ref Vector3 playerPosition)
        {
            if (destinationIsland != null)
            {
                // no movement during jump
                return;
            }

            if (controllerInput.moveStickMoved)
            {
                {
                    movedAt = at;

                    float frozenMultiplier = 1.0f;
                    if (player.GetInt(CommonNames.Frozen) > 0)
                    {
                        frozenMultiplier = 1 / constants.GetFloat("frozen_slowdown_divisor");
                    }

                    // XZ movement
                    if (activeIsland == null)
                    {
                        if (!HadCollision(Game.Instance.Simulation.Time))
                        {
                            // in air
                            playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_jetpack_multiplier")
                                * frozenMultiplier;
                            playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_jetpack_multiplier")
                                * frozenMultiplier;
                        }
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
                                player.SetFloat(CommonNames.Energy, player.GetFloat(CommonNames.Energy) - Game.Instance.Simulation.Time.Dt * constants.GetFloat("running_energy_cost_per_second"));

                                playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_run_multiplier")
                                    * frozenMultiplier;
                                playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_run_multiplier")
                                    * frozenMultiplier;

                                player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "run";
                            }
                            else
                            {
                                playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_walk_multiplier")
                                    * frozenMultiplier;
                                playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_walk_multiplier")
                                    * frozenMultiplier;

                                player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "walk";
                            }

                            // check position a bit further in walking direction to be still on island
                            float islandNonWalkingRangeMultiplier = constants.GetFloat("island_non_walking_range_multiplier")
                                * activeIsland.GetVector3(CommonNames.Scale).X / 100; // normalized to scale of 100
                            Vector3 checkPos = playerPosition + new Vector3(controllerInput.leftStickX * islandNonWalkingRangeMultiplier,
                                0, controllerInput.leftStickY * islandNonWalkingRangeMultiplier); // todo: extract constant

                            Vector3 isectPt;
                            if (!Simulation.GetPositionOnSurface(ref checkPos, activeIsland, out isectPt))
                            {
                                // check point outside of island -> prohibit movement
                                playerPosition = player.GetVector3(CommonNames.PreviousPosition);

                                // plus a bit further inwards
                                Vector3 corrector = (activeIsland.GetVector3(CommonNames.Position) - playerPosition);
                                corrector.Y = 0;
                                if (corrector != Vector3.Zero)
                                {
                                    corrector.Normalize();
                                    corrector.X *= constants.GetFloat("island_non_walking_inwards_acceleration") * dt / 2;
                                    corrector.Z *= constants.GetFloat("island_non_walking_inwards_acceleration") * dt / 2;
                                    inwardsPushVelocity += corrector;
                                    inwardsPushStartedAt = Game.Instance.Simulation.Time.At;
                                }
                            }
                        }
                }
                
                // rotation
                if (destinationIsland == null)
                {
                    float yRotation = (float)Math.Atan2(controllerInput.leftStickX, controllerInput.leftStickY);
                    Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                    player.SetQuaternion(CommonNames.Rotation, Quaternion.CreateFromRotationMatrix(rotationMatrix));
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

                    player.GetProperty<RobotRenderProperty>("render").Squash();
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
                    player.SetQuaternion(CommonNames.Rotation, Quaternion.CreateFromRotationMatrix(rotationMatrix));

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
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.JumpEnd);

            if (won)
            {
                player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "win";
            }
            else
            {
                player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
            }
            destinationIsland.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged -= IslandPositionHandler;
            destinationIsland = null;
        }

        private void PerformJetpackMovement(SimulationTime simTime, float dt, ref Vector3 playerVelocity)
        {
            if ((controllerInput.jumpButtonPressed || controllerInput.jumpButtonHold)
                && activeIsland == null // only in air
                && destinationIsland == null // not while jump
                && simpleJumpIsland == null // dito
                && flame == null // not in combination with flame
                && player.GetInt(CommonNames.Frozen) <= 0 // jetpack doesn't work when frozen
            )
            {
                LeaveActiveIsland();

                player.GetProperty<HUDProperty>("hud").JetpackUsable = false;
                player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";

                if (!jetpackActive)
                {
                    jetpackSoundInstance = Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.JetpackStart);
                    jetpackActive = true;
                }

                playerVelocity += constants.GetVector3("jetpack_acceleration") * dt;

                // deaccelerate the higher we get
                // todo: extract constants (450 & 5 & 10)
                float dist = constants.GetFloat("jetpack_max_height") - player.GetVector3(CommonNames.Position).Y;
                Vector3 deacceleration = constants.GetVector3("jetpack_acceleration") * 6 / dist;
                if (dist < 10) // todo: extract constant
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
            if (player.GetFloat(CommonNames.Health) <= 0)
            {
                if (false/*respawnStartedAt == 0*/)
                {
                    //respawnStartedAt = at;

                    ResetVibration();

                    if (jetpackSoundInstance != null)
                        Game.Instance.AudioPlayer.Stop(jetpackSoundInstance);
                    if (flameThrowerSoundInstance != null)
                        Game.Instance.AudioPlayer.Stop(flameThrowerSoundInstance);
                    jetpackActive = false;
                    if (destinationIsland != null)
                        StopIslandJump();
                    if (repulsionActive)
                        StopRepulsion();
                    LeaveActiveIsland();
                    if (simpleJumpIsland != null)
                        StopSimpleJump();

                    Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.PlayerDies);
                    player.SetInt("deaths", player.GetInt("deaths") + 1);
                    player.SetInt(CommonNames.Lives, player.GetInt(CommonNames.Lives) - 1);

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

                    Vector3 pos = player.GetVector3(CommonNames.Position);
                    deathExplosion.AddVector3Attribute(CommonNames.Position, pos);

                    Game.Instance.Simulation.EntityManager.AddDeferred(deathExplosion, "player_explosion_base", templates);

                    // any lives left?
                    if (player.GetInt(CommonNames.Lives) <= 0 && !won)
                    {
                        Game.Instance.Simulation.EntityManager.EntityRemoved -= EntityRemovedHandler;
                        Game.Instance.Simulation.EntityManager.RemoveDeferred(player);
                    }

                    // dead
                    return true;
                }
                else
                {
                    if (false/*respawnStartedAt + constants.GetInt("respawn_time") >= at*/)
                    {
                        // still dead
                        return true;
                    }
                    else
                    {
                        // reset
                        player.SetQuaternion(CommonNames.Rotation, Quaternion.Identity);
                        player.SetVector3(CommonNames.Velocity, Vector3.Zero);

                        player.SetVector3("collision_pushback_velocity", Vector3.Zero);
                        player.SetVector3("hit_pushback_velocity", Vector3.Zero);

                        player.SetFloat(CommonNames.Energy, constants.GetFloat(CommonNames.MaxEnergy));
                        player.SetFloat(CommonNames.Health, constants.GetFloat(CommonNames.MaxHealth));

                        player.SetInt(CommonNames.Frozen, 0);

                        // random island selection
                        // PositionOnRandomIsland();

                        // activate
                        player.AddProperty("collision", new CollisionProperty(), true);
                        player.AddProperty("render", new RobotRenderProperty(), true);
                        //player.AddProperty("shadow_cast", new ShadowCastProperty());
                        player.GetProperty<CollisionProperty>("collision").OnContact += PlayerCollisionHandler;

                        // indicate
                        if (won == true)
                        {
                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "win";
                        }
                        else
                        {
                            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
                        }

                        // reset respawn timer
//                        respawnStartedAt = 0;
                        // landedAt = -1;
                        // player.SetBool("isRespawning", true);

                        // add light
//                        AddSpawnLight(player);

                        // alive again
                        return true;
                    }
                }
            }

            // not dead
            return false;
        }

        private void PlayerCollisionHandler(SimulationTime simTime, Contact contact)
        {
            if (contact.EntityB.HasAttribute(CommonNames.Kind))
            {
                // slide on jump
                if (destinationIsland != null)
                {
                    // hack: no response on jump (who sees that anyway...)
                    return;

                    /*if (contact.EntityB == destinationIsland)
                        return;

                    Vector3 position = destinationIsland.GetVector3(CommonNames.Position);

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
//                    player.SetVector3(CommonNames.Velocity, player.GetVector3(CommonNames.Velocity) - constants.GetVector3("gravity_acceleration") * simTime.Dt);

                    return;*/
                }
                if (contact.EntityB == activeIsland)
                {
                    // absolutely ignore any collisin with activeisland
                    return;
                }


                String kind = contact.EntityB.GetString(CommonNames.Kind);
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

            // get theoretical position on island
            Vector3 playerPosition = player.GetVector3(CommonNames.Position);
            Vector3 surfacePosition;
            bool canStand = Simulation.GetPositionOnSurface(ref playerPosition, island, out surfacePosition);

            // on top?
            // todo: extract constants
            if (canStand  && (surfacePosition.Y - 10 < playerPosition.Y
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
                    if (player.GetVector3(CommonNames.Velocity).Y > 0)
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
                player.SetVector3(CommonNames.Velocity, Vector3.Zero);

                // position
                Vector3 isectPt;
                if (Simulation.GetPositionOnSurface(ref playerPosition, island, out isectPt))
                {
                    // set position to contact point
                    playerPosition.Y = isectPt.Y + 1;
                    player.SetVector3(CommonNames.Position, playerPosition);
                }
            }
            else
            {
                Vector3 pos = player.GetVector3(CommonNames.Position);
                if (activeIsland != null)
                {
                    // on island -> calculate pseudo normal in xz
                    Vector3 normal = island.GetVector3(CommonNames.Position) - pos;
                    normal.Y = 0;
                    if (normal != Vector3.Zero)
                    {
                        normal.Normalize();
                        Vector3 acc = -normal * 1000; // todo: extract constant
                        player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") +
                            acc * simTime.Dt);
                    }
                }
                else
                if (simpleJumpIsland != null)
                {
                    // don't push in xz if in simplejump
                    Vector3 normal = island.GetVector3(CommonNames.Position) - pos;
                    normal.X = 0;
                    normal.Z = 0;
                    if (normal != Vector3.Zero)
                    {
                        Vector3 acc = -normal * 2000; // todo: extract constant
                        player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") +
                            acc * simTime.Dt);
                    }
                }
                else
                {
                    // in air --> pushback
                    Vector3 acc = -contact[0].Normal * 1000; // todo: extract constant
                    if (acc.Y > 0) // hack: never push upwards
                        acc.Y = -acc.Y;
                    player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity")
                        + acc * simTime.Dt);

                    // slow down jetpack
                    Vector3 velocity = player.GetVector3(CommonNames.Velocity);
                    Vector3 newVelocity = velocity - velocity * 0.9f * simTime.Dt * 25;
                    player.SetVector3(CommonNames.Velocity, newVelocity);
                }
            }
        }

        private void PlayerPillarCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            // only collide with pillar when in air
            if (activeIsland == null)
            {
                Vector3 normal = pillar.GetVector3(CommonNames.Position) - player.GetVector3(CommonNames.Position);
                normal.Y = 0;
                if (normal != Vector3.Zero)
                    normal.Normalize();
                // todo: extract constant
                Vector3 acc = -normal * 2000;
                player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity")
                    + acc * simTime.Dt);
            }
        }

        private void PlayerCaveCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            // only collide with cave when in air
            float factor = (activeIsland != null)?4000f:1000f;
//            if (activeIsland == null)
            {
                Vector3 pos = player.GetVector3(CommonNames.Position);
                pos.Y = 0;
                Vector3 acc = -Vector3.Normalize(pos) * factor; // todo: extract constants
                player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity")
                    + acc * simTime.Dt);
            }
        }

        private void PlayerLavaCollisionHandler(SimulationTime simTime, Entity player, Entity lava)
        {
            if (player.GetVector3(CommonNames.Velocity).Y < 0) // stop downwards velocity
            {
                Vector3 velocity = player.GetVector3(CommonNames.Velocity);
                // todo: extract constant
                player.SetVector3(CommonNames.Velocity, velocity - velocity * 0.9f * simTime.Dt * 25);
            }

            player.SetFloat(CommonNames.Health, player.GetFloat(CommonNames.Health) - simTime.Dt * constants.GetFloat("lava_damage_per_second"));
        }

        private void PlayerPlayerCollisionHandler(SimulationTime simTime, Entity player, Entity otherPlayer, Contact c)
        {
            Vector3 normal = otherPlayer.GetVector3(CommonNames.Position) - player.GetVector3(CommonNames.Position);
            normal.Y = 0;
            if(normal != Vector3.Zero)
                normal.Normalize();

            // and hit?
            if (simTime.At < controllerInput.hitButtonPressedAt + constants.GetInt("hit_cooldown") // has button been pressed lately
                && simTime.At > hitPerformedAt + constants.GetInt("hit_cooldown") // but no hit performed
                && player.GetVector3("hit_pushback_velocity") == Vector3.Zero) // and we have not been hit ourselves
            {
                // indicate hit!
                Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.MeleeHit);

                // deduct health
                otherPlayer.SetFloat(CommonNames.Health, otherPlayer.GetFloat(CommonNames.Health) - constants.GetFloat("hit_damage"));
                CheckPlayerAttributeRanges(otherPlayer);

                // set values
                // todo: bring back up vector! -> needs change in psotion in island too
                Vector3 velocity = (normal /*+ Vector3.UnitY*/) * constants.GetVector3("hit_pushback_velocity_multiplier");
                otherPlayer.SetVector3("hit_pushback_velocity", velocity);
                otherPlayer.SetVector3(CommonNames.Position, otherPlayer.GetVector3(CommonNames.Position) + velocity * simTime.Dt);
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
                    float pr = (player.GetVector3(CommonNames.Scale) * new Vector3(1, 0, 1)).Length();
                    float or = (otherPlayer.GetVector3(CommonNames.Scale) * new Vector3(1, 0, 1)).Length();
                    float dist = ((player.GetVector3(CommonNames.Position) - otherPlayer.GetVector3(CommonNames.Position)) * new Vector3(1, 0, 1)).Length();
                    float delta = or + pr - dist;

                    // Console.WriteLine("putout delta: " + delta);
                    if (delta < 0)
                        delta = 0;

                    player.SetVector3(CommonNames.Position, player.GetVector3(CommonNames.Position) - normal * delta);
                }
            }
        }

        private void HitPushbackEndedHandler()
        {
            canFallFromIsland = false;
        }

        private void HealthChangeHandler(FloatAttribute sender, float oldValue, float newValue)
        {
            if (newValue < oldValue
                && oldValue <= constants.GetFloat(CommonNames.MaxHealth)
                && oldValue > 0) // and not dead yet
            {
                // damage taken
                if (oldValue - newValue >= constants.GetFloat("ice_spike_damage"))
                {
                    player.GetProperty<RobotRenderProperty>("render").NextOnceState = "hurt_hard";
                    Vibrate(1f, 1f);
                }
                else
                {
                    player.GetProperty<RobotRenderProperty>("render").NextOnceState = "hurt_soft";
                    Vibrate(0.5f, 0.5f);
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

            if (entity.HasAttribute(CommonNames.Kind)
                && entity.GetString(CommonNames.Kind) == "icespike"
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
            if (entity.HasAttribute(CommonNames.Kind) && entity.GetString(CommonNames.Kind) == "player")
            {
                // check if we are last man standing
                if (Game.Instance.Simulation.PlayerManager.Count == 1 && 
                    (Game.Instance.Simulation.Phase == SimulationPhase.Intro || Game.Instance.Simulation.Phase == SimulationPhase.Game))
                {
                    won = true;

                    Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.AndTheWinnerIs);

                    if (player.HasProperty("render") && activeIsland != null)
                    {
                        player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "win";
                    }

                    if (selectedIsland != null)
                    {
                        RemoveSelectionArrow();
                    }

                    // remove hud
                    player.RemoveProperty("hud");

                    if (flame != null)
                    {
                        // remove flamethrower flame
                        Game.Instance.Simulation.EntityManager.RemoveDeferred(flame);
                        flame = null;
                    }

                    Entity winningPlayer = Game.Instance.Simulation.PlayerManager[0];

                    Game.Instance.Simulation.SetPhase(
                        SimulationPhase.Outro,
                        winningPlayer.GetString(CommonNames.PlayerName),
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

                Vector3 islandDir = island.GetVector3(CommonNames.Position) - player.GetVector3(CommonNames.Position);
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
                    Vector3 pp = p.GetVector3(CommonNames.Position);
                    Vector3 pdir = pp - playerPosition;
                    Vector3 pdirxz = pdir;
                    pdirxz.Y = 0;
                    if (pdirxz != Vector3.Zero)
                        pdirxz.Normalize();
                    float a = (float)(Math.Acos(Vector3.Dot(pdirxz, direction)) / Math.PI * 180);
                    if (a < maxAngle)
                    {
                        if (a < minAngle)
                        {
                            targetPlayer = p;
                            aimVector = pdir;
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
        /// sets the activeisland
        /// </summary>
        protected override void SetActiveIsland(Entity island)
        {
            canFallFromIsland = false;

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

            base.SetActiveIsland(island);
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
                    if (player.HasProperty("hud"))
                    {
                        player.GetProperty<HUDProperty>("hud").JetpackUsable = true;
                    }
                }

                activeIsland.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged -= IslandPositionHandler;
                activeIsland.SetInt("players_on_island", activeIsland.GetInt("players_on_island") - 1);
                // ensure repulsion is reset
                if (repulsionActive)
                    StopRepulsion();
                // ensure repulsed_by is reset
                if(activeIsland.GetString("repulsed_by") == player.Name)
                    activeIsland.SetString("repulsed_by", player.Name);


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
            Game.Instance.AudioPlayer.Play(Game.Instance.Simulation.SoundRegistry.JumpEnd);

            simpleJumpIsland.GetAttribute<Vector3Attribute>(CommonNames.Position).ValueChanged -= IslandPositionHandler;
            player.GetProperty<RobotRenderProperty>("render").NextPermanentState = "idle";
            player.SetString("jump_island", "");
            simpleJumpIsland = null;
        }

        public static Vector3 GetPosition(Entity player)
        {
            return player.GetVector3(CommonNames.Position);
        }

        public static Vector3 GetScale(Entity player)
        {
            if (player.HasVector3(CommonNames.Scale))
            {
                return player.GetVector3(CommonNames.Scale);
            }
            else
            {
                return Vector3.One;
            }
        }

        public static Quaternion GetRotation(Entity player)
        {
            if (player.HasQuaternion(CommonNames.Rotation))
            {
                return player.GetQuaternion(CommonNames.Rotation);
            }
            else
            {
                return Quaternion.Identity;
            }
        }
    }
}

