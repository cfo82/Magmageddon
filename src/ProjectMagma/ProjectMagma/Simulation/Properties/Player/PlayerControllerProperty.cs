using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation
{
    public class PlayerControllerProperty : Property
    {
        private Entity player;
        private Entity constants;

        private Entity activeIsland = null;

        private readonly Random rand = new Random(DateTime.Now.Millisecond);
        private double respawnStartedAt = 0;

        private bool jetpackActive = false;

        private int iceSpikeCount = 0;
        private int iceSpikeRemovedCount = 0;
        private float iceSpikeFiredAt = 0;

        private double hitPerformedAt = 0;

        private float islandRepulsionStoppedAt = 0;

        private float islandSelectedAt = 0;
        private Vector3 lastStickDir = Vector3.Zero;
        private Entity selectedIsland = null;
        private Entity destinationIsland = null;
        private Vector3 lastIslandDir = Vector3.Zero;
        private float islandJumpPerformedAt = 0;
        private bool jumpButtonReleased = true;
        private Entity attractedIsland = null;

        Entity flame = null;
        Entity arrow;

        private SoundEffect jetpackSound;
        private SoundEffectInstance jetpackSoundInstance;
        private SoundEffect flameThrowerSound;
        private SoundEffectInstance flameThrowerSoundInstance;

        // values which get reset on each update
        private bool collisionOccured = false;
        private bool movedByStick = false;
        Vector3 previousPosition;

        public PlayerControllerProperty()
        {
        }

        public void OnAttached(Entity player)
        {
            player.Update += OnUpdate;

            this.player = player;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

            player.AddQuaternionAttribute("rotation", Quaternion.Identity);
            player.AddVector3Attribute("velocity", Vector3.Zero);

            player.AddVector3Attribute("collision_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("player_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddVector3Attribute("island_jump_velocity", Vector3.Zero);

            player.AddIntAttribute("energy", constants.GetInt("max_energy"));
            player.AddIntAttribute("health", constants.GetInt("max_health"));
            player.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            player.AddIntAttribute("kills", 0);
            player.AddIntAttribute("deaths", 0);

            player.AddIntAttribute("frozen", 0);
            player.AddStringAttribute("active_island", "");

            Game.Instance.Simulation.EntityManager.EntityRemoved += EntityRemovedHandler;
            ((CollisionProperty)player.GetProperty("collision")).OnContact += PlayerCollisionHandler;

            jetpackSound = Game.Instance.Content.Load<SoundEffect>("Sounds/jetpack");
            flameThrowerSound = Game.Instance.Content.Load<SoundEffect>("Sounds/flamethrower");

            arrow = new Entity("arrow" + "_" + player.Name);
            arrow.AddStringAttribute("player", player.Name);

            arrow.AddVector3Attribute("position", Vector3.Zero);
            arrow.AddStringAttribute("island", "");

            arrow.AddStringAttribute("mesh", player.GetString("arrow_mesh"));
            arrow.AddVector3Attribute("scale", new Vector3(12, 12, 12));

            arrow.AddProperty("arrow_controller_property", new ArrowControllerProperty());

            Game.Instance.Simulation.EntityManager.AddDeferred(arrow);

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
            if (!collisionOccured)
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
            movedByStick = false;

            // get input
            controllerInput.Update(playerIndex);

            #region movement

            // jetpack
            PerformJetpackMovement(simTime, dt, ref playerVelocity, ref fuel);

            // perform island jump
            PerformIslandJump(player, dt, at, ref playerPosition, ref playerVelocity);

            // only apply velocity if not on island
            ApplyGravity(dt, ref playerPosition, ref playerVelocity);

            // apply current velocity
            playerPosition += playerVelocity * dt;

//            Console.WriteLine();
//            Console.WriteLine("at: " + (int)gameTime.TotalGameTime.TotalMilliseconds);
//            Console.WriteLine("velocity: " + playerVelocity + " led to change from " + previousPosition + " to " + playerPosition);

            PerformStickMovement(player, dt, ref playerPosition, fuel);

            // pushback
            Simulation.ApplyPushback(ref playerPosition, ref collisionPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));
            Simulation.ApplyPushback(ref playerPosition, ref playerPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));
            Simulation.ApplyPushback(ref playerPosition, ref hitPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));

            // frozen!?
            PerformFrozenSlowdown(player, simTime, ref playerPosition);

            #endregion

            #region actions

            PerformIceSpikeAction(player, at, playerPosition);

            PerformFlamethrowerAction(player, playerPosition);

            PerformIslandRepulsionAction(at, ref fuel);

            // TODO: island selection with islands/player projected in screen-plane.
            // TODO: island selection don't change island if stick has not been moved

            // island selection and attraction
            bool allowSelection = attractedIsland == null
                && destinationIsland == null;
            PerformIslandSelectionAction(at, allowSelection);
            PerformIslandJumpAction(ref playerPosition, ref playerVelocity);
            PerformIslandAttractionAction(player, allowSelection);
            #endregion

            #region recharge
            // recharge energy
            if (flame == null)
            {
                Game.Instance.Simulation.ApplyIntervalAddition(player, "energy_recharge", constants.GetInt("energy_recharge_interval"),
                    player.GetIntAttribute("energy"));
            }

            // recharge fuel
            if (!controllerInput.jetpackButtonPressed)
            {
                if (activeIsland == null)
                {
                    fuel += (int)(simTime.DtMs * constants.GetFloat("fuel_recharge_multiplier"));
                }
                else
                {
                    // faster recharge standing on island, but only if jetpack was not used for repulsion
                    if (at > islandRepulsionStoppedAt + constants.GetFloat("island_repulsion_recharge_delay"))
                    {
                        fuel += (int)(simTime.DtMs * constants.GetFloat("fuel_recharge_multiplier_island"));
                    }
                    else
                    {
                        double diff = at - islandRepulsionStoppedAt;
                        fuel += (int)(simTime.DtMs * constants.GetFloat("fuel_recharge_multiplier_island")
                            * diff / constants.GetFloat("island_repulsion_recharge_delay"));
                    }
                }
            }
            #endregion

            // update player attributes
            player.SetInt("fuel", fuel);

            player.SetVector3("position", playerPosition);
            player.SetVector3("velocity", playerVelocity);
            player.SetVector3("collision_pushback_velocity", collisionPushbackVelocity);
            player.SetVector3("player_pushback_velocity", playerPushbackVelocity);
            player.SetVector3("hit_pushback_velocity", hitPushbackVelocity);

            CheckPlayerAttributeRanges(player);

            // reset stuff
            collisionOccured = false;

            // check collision with lava
            Entity lava = Game.Instance.Simulation.EntityManager["lava"];
            if (playerPosition.Y < lava.GetVector3("position").Y)
                PlayerLavaCollisionHandler(simTime, player, lava);
        }

        private void PerformIslandAttractionAction(Entity player, bool allowSelection)
        {
            // island attraction start
            if (controllerInput.attractionButtonPressed
                && selectedIsland != null
                && allowSelection)
            {
                attractedIsland = selectedIsland;
                attractedIsland.SetString("attracted_by", player.Name);
                ((StringAttribute)attractedIsland.GetAttribute("attracted_by")).ValueChanged += IslandAttracedByChangeHandler;
            }
            else
            // deactivate attraction
            if (attractedIsland != null
                && (!controllerInput.attractionButtonPressed
                || controllerInput.jetpackButtonPressed))
            {
                // will call handler
                attractedIsland.SetString("attracted_by", "");
            }

        }

        private void PerformIslandJumpAction(ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            // island jump start
            if (controllerInput.jetpackButtonPressed
                && selectedIsland != null
                && destinationIsland == null
                && jumpButtonReleased // don't allow jumps by having the jump button pressed all the time
            )
            {
                LeaveActiveIsland();

                destinationIsland = selectedIsland;

                // calculate time to travel to island (in xz plane) using an iterative approach
                float oldTime = 0;
                float time = 0;
                float maxTime = 0;
                Vector3 islandDir;
                Vector3 islandPos = destinationIsland.GetVector3("position");
                do
                {
                    ((IslandControllerPropertyBase)destinationIsland.GetProperty("controller")).CalculateNewPosition(destinationIsland,
                        ref islandPos, time);

                    islandDir = (islandPos - playerPosition);
                    Vector3 islandDir2D = islandDir;
                    //                    islandDir2D.Y = 0;
                    float dist2D = islandDir2D.Length();

                    if (time > maxTime)
                        maxTime = time;

                    oldTime = time;
                    time = dist2D / constants.GetFloat("island_jump_speed");
                }
                while (Math.Abs(oldTime - time) > 1 / 1000f);

                lastIslandDir = islandDir;

                playerVelocity = -constants.GetVector3("gravity_acceleration") * maxTime;
                player.SetVector3("island_jump_velocity", Vector3.Normalize(islandDir) * constants.GetFloat("island_jump_speed"));
            }
        }

        private void PerformIslandSelectionAction(float at, bool allowSelection)
        {
            if (allowSelection)
            {
                if (controllerInput.rightStickMoved
                    && activeIsland != null
                    && selectedIsland != activeIsland) // must be standing on island
                {
                    Vector3 stickDir = new Vector3(controllerInput.rightStickX, 0, controllerInput.rightStickY);
                    stickDir.Normalize();
                    // only allow reselection if stick moved slightly
                    bool stickMoved = Vector3.Dot(lastStickDir, stickDir) < constants.GetFloat("island_reselection_max_value");
                    if ((selectedIsland == null || stickMoved)
                        && at > islandSelectedAt + constants.GetFloat("island_reselection_timeout"))
                    {
                        //                        Console.WriteLine("new selection (old: " + ((selectedIsland != null) ? selectedIsland.Name : "") + "): " + lastStickDir + "." + stickDir + " = " +
                        //                            Vector3.Dot(lastStickDir, stickDir));

                        // select closest island in direction of stick
                        lastStickDir = stickDir;
                        selectedIsland = SelectBestIsland(stickDir);

                        if (selectedIsland != null)
                        {
                            islandSelectedAt = at;
                            arrow.SetString("island", selectedIsland.Name);

                            if (!arrow.HasProperty("render"))
                            {
                                arrow.AddProperty("render", new BasicRenderProperty());
                                arrow.AddProperty("shadow_cast", new ShadowCastProperty());
                            }
                        }
                    }
                }
                else
                {
                    // deselect after timeout
                    if (selectedIsland != null
                        && at > islandSelectedAt + constants.GetFloat("island_deselection_timeout"))
                    {
                        selectedIsland = null;
                        arrow.RemoveProperty("render");
                        arrow.RemoveProperty("shadow_cast");
                        islandSelectedAt = 0;
                    }
                }
            }
        }

        private void PerformIslandRepulsionAction(float at, ref int fuel)
        {
            // island repulsion
            if (controllerInput.dPadPressed
                && activeIsland != null
                && fuel > constants.GetInt("island_repulsion_fuel_cost"))
            {
                float velocityMultiplier = constants.GetFloat("island_repulsion_velocity_multiplier");
                Vector3 velocity = new Vector3(controllerInput.dPadX * velocityMultiplier, 0, controllerInput.dPadY * velocityMultiplier);
                activeIsland.SetVector3("repulsion_velocity", activeIsland.GetVector3("repulsion_velocity") + velocity);

                fuel -= constants.GetInt("island_repulsion_fuel_cost");

                islandRepulsionStoppedAt = at;
            }
        }

        private void PerformFlamethrowerAction(Entity player, Vector3 playerPosition)
        {
            // flamethrower
            if (controllerInput.flamethrowerButtonPressed
                && activeIsland != null) // only allowed on ground
            {
                if (flame == null)
                {
                    if (player.GetInt("energy") > constants.GetInt("flamethrower_warmup_energy_cost"))
                    {
                        // indicate 
                        flameThrowerSoundInstance = flameThrowerSound.Play(Game.Instance.EffectsVolume, 1, 0, true);

                        Vector3 pos = new Vector3(playerPosition.X + 10, playerPosition.Y + 10, playerPosition.Z);
                        Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                        flame = new Entity("flame" + "_" + player.Name);
                        flame.AddStringAttribute("player", player.Name);
                        flame.AddBoolAttribute("active", false);

                        flame.AddVector3Attribute("velocity", viewVector);
                        flame.AddVector3Attribute("position", pos);

                        flame.AddStringAttribute("mesh", "Models/Visualizations/flame_primitive");
                        flame.AddVector3Attribute("scale", new Vector3(0, 0, 0));
                        flame.AddVector3Attribute("full_scale", new Vector3(26, 26, 26));
                        flame.AddQuaternionAttribute("rotation", GetRotation(player));

                        flame.AddStringAttribute("bv_type", "sphere");

                        flame.AddProperty("render", new BasicRenderProperty());
                        flame.AddProperty("collision", new CollisionProperty());
                        flame.AddProperty("controller", new FlamethrowerControllerProperty());

                        Game.Instance.Simulation.EntityManager.AddDeferred(flame);
                    }
                }
                else
                    if (player.GetInt("energy") <= 0)
                        flame.SetBool("fueled", false);
            }
            else
                if (flame != null)
                    flame.SetBool("fueled", false);
        }

        private void PerformIceSpikeAction(Entity player, float at, Vector3 playerPosition)
        {
            // ice spike
            if (controllerInput.iceSpikeButtonPressed && player.GetInt("energy") > constants.GetInt("ice_spike_energy_cost")
                && (at - iceSpikeFiredAt) > constants.GetInt("ice_spike_cooldown"))
            {
                // indicate 
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
                soundEffect.Play(Game.Instance.EffectsVolume);

                // todo: specify point in model
                Vector3 pos = new Vector3(playerPosition.X + 5, playerPosition.Y + 10, playerPosition.Z + 5);
                Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player));

                #region search next player in range

                float angle = constants.GetFloat("ice_spike_aim_angle");
                float aimDistance = float.PositiveInfinity;
                Entity targetPlayer = null;
                Vector3 distVector = Vector3.Zero;
                foreach (Entity p in Game.Instance.Simulation.PlayerManager)
                {
                    if (p != player)
                    {
                        Vector3 pp = p.GetVector3("position");
                        Vector3 pdir = pp - playerPosition;
                        float a = (float)(Math.Acos(Vector3.Dot(pdir, viewVector) / pdir.Length() / viewVector.Length()) / Math.PI * 180);
                        if (a < angle)
                        {
                            float ad = pdir.Length();
                            if (ad < aimDistance)
                            {
                                targetPlayer = p;
                                distVector = pdir;
                                aimDistance = ad;
                            }
                        }
                    }
                }
                String targetPlayerName = targetPlayer != null ? targetPlayer.Name : "";
                //Console.WriteLine("targetPlayer: " + targetPlayerName);

                #endregion

                Vector3 aimVector = viewVector;
                if (targetPlayer != null)
                {
                    aimVector = Vector3.Normalize(distVector);
                }
                aimVector *= constants.GetFloat("ice_spike_speed");

                Entity iceSpike = new Entity("icespike" + (++iceSpikeCount) + "_" + player.Name);
                iceSpike.AddStringAttribute("player", player.Name);
                iceSpike.AddStringAttribute("target_player", targetPlayerName);
                iceSpike.AddIntAttribute("creation_time", (int)at);

                iceSpike.AddVector3Attribute("velocity", aimVector);
                iceSpike.AddVector3Attribute("position", pos);

                iceSpike.AddStringAttribute("mesh", "Models/Visualizations/icespike_primitive");
                iceSpike.AddVector3Attribute("scale", new Vector3(5, 5, 5));

                iceSpike.AddStringAttribute("bv_type", "sphere");

                iceSpike.AddProperty("render", new BasicRenderProperty());
                iceSpike.AddProperty("collision", new CollisionProperty());
                iceSpike.AddProperty("controller", new IceSpikeControllerProperty());

                Game.Instance.Simulation.EntityManager.AddDeferred(iceSpike);

                // update states
                player.SetInt("energy", player.GetInt("energy") - constants.GetInt("ice_spike_energy_cost"));
                iceSpikeFiredAt = at;
            }
        }

        private void PerformFrozenSlowdown(Entity player, SimulationTime simTime, ref Vector3 playerPosition)
        {
            if (player.GetInt("frozen") > 0)
            {
                playerPosition = (previousPosition + playerPosition) / 2;
                player.SetInt("frozen", player.GetInt("frozen") - (int)simTime.DtMs);
                if (player.GetInt("frozen") < 0)
                    player.SetInt("frozen", 0);
            }
        }

        private void PerformStickMovement(Entity player, float dt, ref Vector3 playerPosition, int fuel)
        {
            if (controllerInput.moveStickMoved)
            {
                movedByStick = true;

                // XZ movement
                if (activeIsland == null)
                {
                    // in air
                    playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_jetpack_multiplier");
                    playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_jetpack_multiplier");
                }
                else
                {
                    // on island ground
                    playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_movement_multiplier");
                    playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_movement_multiplier");

                    // position on island surface
                    Vector3 isectPt = Vector3.Zero;
                    Ray3 ray = new Ray3(playerPosition + 1000 * Vector3.UnitY, -Vector3.UnitY);
                    if (Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, activeIsland, out isectPt))
                    {
                        // set position to contact point
                        playerPosition.Y = isectPt.Y + 1; // todo: make constant?
                        player.SetVector3("position", playerPosition);
                    }
                    else
                    // not over island anymore -> don't allow movement
                    {
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
                if (activeIsland != null)
                {
                    // position on island surface
                    Vector3 isectPt = Vector3.Zero;
                    Ray3 ray = new Ray3(playerPosition + 1000 * Vector3.UnitY, -Vector3.UnitY);
                    if (Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, activeIsland, out isectPt))
                    {
                        // set position to contact point
                        playerPosition.Y = isectPt.Y + 1; // todo: make constant?
                        player.SetVector3("position", playerPosition);
                    }
                    else
                    // not over island anymore
                    {
                        LeaveActiveIsland();
                    }
                }
            }
        }

        private void ApplyGravity(float dt, ref Vector3 playerPosition, ref Vector3 playerVelocity)
        {
            if (activeIsland == null)
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

                // check for arrival
                Vector3 islandDir = destinationIsland.GetVector3("position") - playerPosition;
                Vector3 isectPt = Vector3.Zero;
                Ray3 ray = new Ray3(playerPosition, -Vector3.UnitY);
                if (Vector3.Dot(islandDir, lastIslandDir) < 0 // oscillation?
                   && Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, destinationIsland, out isectPt))
                {
                    SetActiveIsland(destinationIsland);

                    playerPosition = isectPt;

                    destinationIsland = null;
                    playerVelocity = Vector3.Zero;
                    islandJumpPerformedAt = at;
                    jumpButtonReleased = false;
                }
                else
                {
                    // not yet -> calculate velocity for jump

                    float yRotation = (float)Math.Atan2(islandDir.X, islandDir.Z);
                    Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                    player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));

                    lastIslandDir = islandDir;

                    player.SetVector3("island_jump_velocity", Vector3.Normalize(islandDir) * constants.GetFloat("island_jump_speed"));
                }
            }
            else
            {
                // jumpButtonReleased gets set to true as soon as the jump button is not pressed anymore
                if (!controllerInput.jetpackButtonPressed)
                    jumpButtonReleased = true;
            }
        }

        private void PerformJetpackMovement(SimulationTime simTime, float dt, ref Vector3 playerVelocity, ref int fuel)
        {
            if (controllerInput.jetpackButtonPressed
                && activeIsland == null
                && selectedIsland == null
                && destinationIsland == null
                && flame == null
                && fuel > 0
            )
            {
                if (!jetpackActive)
                {
                    jetpackSoundInstance = jetpackSound.Play(0.4f * Game.Instance.EffectsVolume, 1, 0, true);
                    jetpackActive = true;
                }

                // todo: add constant that can modify this
                fuel -= (int)simTime.DtMs;
                playerVelocity += constants.GetVector3("jetpack_acceleration") * dt;

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
        /// checks if player's health has fallen below 0, then perform respawn
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
                    selectedIsland = null;
                    if (attractedIsland != null)
                        attractedIsland.SetString("attracted_by", "");
                    attractedIsland = null;
                    destinationIsland = null;
                    LeaveActiveIsland();

                    Game.Instance.Content.Load<SoundEffect>("Sounds/death").Play(Game.Instance.EffectsVolume);
                    player.SetInt("deaths", player.GetInt("deaths") + 1);

                    // deactivate
                    player.RemoveProperty("render");
                    player.RemoveProperty("shadow_cast");
                    player.RemoveProperty("collision");

                    if (arrow.HasProperty("render"))
                    {
                        arrow.RemoveProperty("render");
                        arrow.RemoveProperty("shadow_cast");
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
                        player.AddProperty("render", new BasicRenderProperty());
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

        private void CheckPlayerAttributeRanges(Entity player)
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
        }

        private void PlayerCollisionHandler(SimulationTime simTime, Contact contact)
        {
            if (contact.EntityB.HasAttribute("kind"))
            {
                // slide
                if (destinationIsland != null
                    && (contact.EntityB.GetString("kind") == "pillar"
                    || contact.EntityB.GetString("kind") == "island"))
                {
                    if (contact.EntityB == destinationIsland)
                        return;

                    Vector3 position = previousPosition + player.GetVector3("island_jump_velocity") * simTime.Dt;

                    // get distance of destination point to sliding plane
                    float distance = Vector3.Dot(contact[0].Normal, position - contact[0].Point);

                    // calculate point on plane
                    Vector3 cpos = position - contact[0].Normal * distance;

                    Vector3 slidingDelta = cpos - contact[0].Point;
                    Vector3 slidingVelocity = slidingDelta / simTime.Dt;

                    Console.WriteLine("orignal velocity " + player.GetVector3("island_jump_velocity") + "; sliding velocity: " + slidingVelocity);

                    //                    slidingVelocity.Y = player.GetVector3("island_jump_velocity").Y;
                    slidingVelocity.Y = player.GetVector3("island_jump_velocity").Y;
                    player.SetVector3("island_jump_velocity", slidingVelocity);

                    // also ensure we don't fall down yet
                    player.SetVector3("velocity", player.GetVector3("velocity") - constants.GetVector3("gravity_acceleration") * simTime.Dt);

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
                    case "player":
                        PlayerPlayerCollisionHandler(simTime, contact.EntityA, contact.EntityB, contact);
                        break;
                }
                CheckPlayerAttributeRanges(player);
                collisionOccured = true;
            }
        }

        private void PlayerIslandCollisionHandler(SimulationTime simTime, Entity player, Entity island, Contact contact)
        {
            float dt = simTime.Dt;

            if (island == destinationIsland)
            {
                // ignore collision with destinationisland
                return;
            }

            // on top?
            if (Vector3.Dot(Vector3.UnitY, contact[0].Normal) < 0)
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
                Vector3 isectPt = Vector3.Zero;
                Ray3 ray = new Ray3(player.GetVector3("position") + 1000 * Vector3.UnitY, -Vector3.UnitY);
                if (Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, island, out isectPt))
                {
                    // set position to contact point
                    Vector3 pos = player.GetVector3("position");
                    pos.Y = isectPt.Y;
                    player.SetVector3("position", pos);
                }
            }
            else
            {
                Vector3 pos = player.GetVector3("position");
                Vector3 velocity = -contact[0].Normal * (pos - previousPosition).Length() / simTime.Dt;
                player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
            }
        }

        private void PlayerPillarCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            Vector3 pos = player.GetVector3("position");
            Vector3 velocity = -co[0].Normal * (pos - previousPosition).Length() / simTime.Dt;
            player.SetVector3("collision_pushback_velocity", player.GetVector3("collision_pushback_velocity") + velocity);
        }

        private void PlayerLavaCollisionHandler(SimulationTime simTime, Entity player, Entity lava)
        {
            Game.Instance.Simulation.ApplyPerSecondSubstraction(player, "lava_damage", constants.GetInt("lava_damage_per_second"),
                player.GetIntAttribute("health"));
        }

        private void PlayerPlayerCollisionHandler(SimulationTime simTime, Entity player, Entity otherPlayer, Contact c)
        {
            Console.WriteLine(player.Name + " collided with " + otherPlayer.Name);
            // and hit?
            if (controllerInput.hitButtonPressed &&
                simTime.At > hitPerformedAt + constants.GetInt("hit_cooldown"))
            {
                // indicate hit!
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
                soundEffect.Play(Game.Instance.EffectsVolume);

                // deduct health
                otherPlayer.SetInt("health", otherPlayer.GetInt("health") - constants.GetInt("hit_damage"));
                CheckPlayerAttributeRanges(otherPlayer);

                // set values
                otherPlayer.SetVector3("hit_pushback_velocity", c[0].Normal * constants.GetFloat("hit_pushback_velocity_multiplier"));
                hitPerformedAt = simTime.At;
            }
            else
            {
                // normal feedback
                if ((previousPosition - player.GetVector3("position")).Length() > 0.1) // apply feedback to player that changed its position
                {
                    player.SetVector3("player_pushback_velocity", /*player.GetVector3("player_pushback_velocity")*/
                        - c[0].Normal * constants.GetFloat("player_pushback_velocity_multiplier"));
                }
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
                arrow.RemoveProperty("render");
                arrow.RemoveProperty("shadow_cast");

                // reset attraction and selection
                attractedIsland = null;
                selectedIsland = null;
            }
        }

        private void EntityRemovedHandler(EntityManager manager, Entity entity)
        {
            if (entity.Name.Equals("flame" + "_" + player.Name))
            {
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
        }

        /// <summary>
        /// selects and island closest to direction dir
        /// </summary>
        /// <param name="dir">direction to select</param>
        /// <returns>an island entity or null</returns>
        private Entity SelectBestIsland(Vector3 dir)
        {
            float closestAngle = float.MaxValue;
            float distance = float.MaxValue;
            Entity selectedIsland = null;
            foreach (Entity island in Game.Instance.Simulation.IslandManager)
            {
                Vector3 islandDir = island.GetVector3("position") - player.GetVector3("position");
                float dist = islandDir.Length();
                islandDir.Y = 0;
                float angle = (float)(Math.Acos(Vector3.Dot(dir, islandDir) / dist));
//                float angle = (float)(Math.Acos(Vector3.Dot(dir, islandDir) / dist) / Math.PI * 180);
                if (island != activeIsland
                    && (angle / Math.PI * 180) < constants.GetFloat("island_aim_angle")) 
                {
                    if(angle < closestAngle
                        || (Math.Abs(angle-closestAngle) < constants.GetFloat("island_aim_angle_eps")
                        && dist < distance))
                    {        
                        selectedIsland = island;
                        closestAngle = angle;
                        distance = dist;
                    }
                }
            }

            return selectedIsland;
        }

        /// <summary>
        /// positions the player randomly on an island
        /// </summary>
        private void PositionOnRandomIsland()
        {
            Entity island;
            for(;;)
            {
                int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                island = Game.Instance.Simulation.IslandManager[islandNo];
                // check no powerup on island
                foreach(Entity powerup in Game.Instance.Simulation.PowerupManager)
                {
                    if (island.Name == powerup.GetString("island_reference"))
                        continue; // select again
                }
                // check island is far enough away of other players
                foreach (Entity p in Game.Instance.Simulation.PlayerManager)
                {
                    if ((island.GetVector3("position") - p.GetVector3("position")).Length() > constants.GetFloat("respawn_min_distance_to_players"))
                        continue; // select again
                }
                // no powerup on selected island -> break;
                break; 
            }
            SetActiveIsland(island);
            player.SetVector3("position", island.GetVector3("position"));
        }

        /// <summary>
        /// sets the activeisland
        /// </summary>
        private void SetActiveIsland(Entity island)
        {
            Console.WriteLine(player.Name + " activated island");

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
                Console.WriteLine(player.Name+" left island");
                ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;
                activeIsland.SetInt("players_on_island", activeIsland.GetInt("players_on_island") - 1);

                activeIsland = null;
                player.SetString("active_island", "");
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

        struct ControllerInput
        {
            public void Update(PlayerIndex playerIndex)
            {
                GamePadState gamePadState = GamePad.GetState(playerIndex);
                KeyboardState keyboardState = Keyboard.GetState(playerIndex);

                #region joysticks

                leftStickX = gamePadState.ThumbSticks.Left.X;
                leftStickY = -gamePadState.ThumbSticks.Left.Y;
                moveStickMoved = leftStickX != 0.0f || leftStickY != 0.0f;
                rightStickX = gamePadState.ThumbSticks.Right.X;
                rightStickY = -gamePadState.ThumbSticks.Right.Y;
                rightStickMoved = rightStickX != 0.0f || rightStickY != 0.0f;
                rightStickPressed = gamePadState.Buttons.RightStick == ButtonState.Pressed;

                dPadX = (gamePadState.DPad.Right == ButtonState.Pressed)? 1.0f : 0.0f
                    - ((gamePadState.DPad.Left == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadY = (gamePadState.DPad.Down == ButtonState.Pressed) ? 1.0f : 0.0f
                    - ((gamePadState.DPad.Up == ButtonState.Pressed) ? 1.0f : 0.0f);
                dPadPressed = dPadX != 0 || dPadY != 0;

                if (!moveStickMoved)
                {
                    if (playerIndex == PlayerIndex.One)
                    {
                        if (keyboardState.IsKeyDown(Keys.A))
                        {
                            leftStickX = gamepadEmulationValue;
                            moveStickMoved = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.D))
                            {
                                leftStickX = -gamepadEmulationValue;
                                moveStickMoved = true;
                            }

                        if (keyboardState.IsKeyDown(Keys.W))
                        {
                            leftStickY = gamepadEmulationValue;
                            moveStickMoved = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.S))
                            {
                                leftStickY = -gamepadEmulationValue;
                                moveStickMoved = true;
                            }
                    }
                    else
                    {
                        if (keyboardState.IsKeyDown(Keys.Left))
                        {
                            leftStickX = gamepadEmulationValue;
                            moveStickMoved = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.Right))
                            {
                                leftStickX = -gamepadEmulationValue;
                                moveStickMoved = true;
                            }

                        if (keyboardState.IsKeyDown(Keys.Up))
                        {
                            leftStickY = -gamepadEmulationValue;
                            moveStickMoved = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.Down))
                            {
                                leftStickY = gamepadEmulationValue;
                                moveStickMoved = true;
                            }
                    }
                }

                #endregion

                #region action buttons

                jetpackButtonPressed =
                    gamePadState.Buttons.A == ButtonState.Pressed ||
                    gamePadState.Triggers.Left > 0 ||
                    (keyboardState.IsKeyDown(Keys.Space) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Insert) && playerIndex == PlayerIndex.Two);

                iceSpikeButtonPressed = gamePadState.Buttons.X == ButtonState.Pressed;
                if((keyboardState.IsKeyDown(Keys.Q) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.RightControl) && playerIndex == PlayerIndex.Two))
                {
                    iceSpikeButtonPressed = true;
                }

                hitButtonPressed =
                    gamePadState.Buttons.RightShoulder == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.E) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Enter) && playerIndex == PlayerIndex.Two);

                flamethrowerButtonPressed =
                    gamePadState.Buttons.Y == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.R) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Delete) && playerIndex == PlayerIndex.Two);

                attractionButtonPressed =
                    gamePadState.Triggers.Right > 0;

                #endregion

            }

            // in order to use the following variables as private with getters/setters, do
            // we really need 15 lines per variable?!
            
            // joystick
            public float leftStickX, leftStickY;
            public bool moveStickMoved;
            public float rightStickX, rightStickY;
            public bool rightStickMoved, rightStickPressed;
            public bool dPadPressed;
            public float dPadX, dPadY;

            // buttons
            public bool jetpackButtonPressed, flamethrowerButtonPressed, iceSpikeButtonPressed, hitButtonPressed, attractionButtonPressed;

            private static float gamepadEmulationValue = -1f;
        }

        ControllerInput controllerInput;
    }


}
