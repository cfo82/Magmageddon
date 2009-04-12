using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public class PlayerControllerProperty : Property
    {
        private Entity player;
        private Entity constants;

        private Entity activeIsland = null;
        private bool islandCollision = false;

        private readonly Random rand = new Random(DateTime.Now.Millisecond);
        private double respawnStartedAt = 0;

        private bool jetpackActive = false;

        private int iceSpikeCount = 0;
        private int iceSpikeRemovedCount = 0;
        private double iceSpikeFiredAt = 0;

        private double hitPerformedAt = 0;

        private double islandLeftAt = 0;
        private float islandRepulsionStoppedAt = 0;

        private double islandSelectedAt = 0;
        private Entity selectedIsland = null;
        private Entity destinationIsland = null;
        private Vector3 lastIslandDir = Vector3.Zero;
        private float islandJumpPerformedAt = 0;
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

            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

            // random island selection
            // TODO: make method
            int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
            Entity island = Game.Instance.Simulation.IslandManager[islandNo];
            Vector3 pos = island.GetVector3("position");
            pos.Y = pos.Y + 30; // todo: change this to point defined in mesh
            player.AddVector3Attribute("position", pos);

            player.AddQuaternionAttribute("rotation", Quaternion.Identity);
            player.AddVector3Attribute("velocity", Vector3.Zero);

            player.AddVector3Attribute("collision_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("player_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddIntAttribute("energy", constants.GetInt("max_energy"));
            player.AddIntAttribute("health", constants.GetInt("max_health"));
            player.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            player.AddIntAttribute("kills", 0);
            player.AddIntAttribute("deaths", 0);

            player.AddIntAttribute("frozen", 0);
            player.AddStringAttribute("collisionPlayer", "");

            Game.Instance.Simulation.EntityManager.EntityRemoved += EntityRemovedHandler;
            if (player.HasProperty("collision"))
            {
                ((CollisionProperty)player.GetProperty("collision")).OnContact += PlayerCollisionHandler;
            }

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

            this.previousPosition = player.GetVector3("position");
            this.player = player;
        }

        public void OnDetached(Entity player)
        {
            player.Update -= OnUpdate;
            Game.Instance.Simulation.EntityManager.Remove(arrow);
            if(flame != null)
                Game.Instance.Simulation.EntityManager.Remove(flame);
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

            #region death
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

                    Game.Instance.Content.Load<SoundEffect>("Sounds/death").Play(Game.Instance.EffectsVolume);
                    player.SetInt("deaths", player.GetInt("deaths") + 1);

                    player.RemoveProperty("render");
                    player.RemoveProperty("shadow_cast");

                    if (arrow.HasProperty("render"))
                    {
                        arrow.RemoveProperty("render");
                        arrow.RemoveProperty("shadow_cast");
                    }

                    return;
                }
                else
                    if (respawnStartedAt + constants.GetInt("respawn_time") >= at)
                    {
                        // do nothing
                        return;
                    }
                    else
                    {
                        // reposition
                        player.AddProperty("render", new RenderProperty());
                        player.AddProperty("shadow_cast", new ShadowCastProperty());

                        // random island selection
                        int islandNo = rand.Next(Game.Instance.Simulation.IslandManager.Count - 1);
                        Entity island = Game.Instance.Simulation.IslandManager[islandNo];
                        Vector3 pos = island.GetVector3("position");
                        pos.Y = pos.Y + 5; // todo: change this to point defined in mesh
                        player.SetVector3("position", pos);

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
                        player.SetString("collisionPlayer", "");

                        respawnStartedAt = 0;
                    }
            }
            #endregion

            #region collision reaction
            // island leave check
            if (islandCollision == false && activeIsland != null)
            {
                if (movedByStick && !jetpackActive)
                {
                    // reset movement, so we cannot fall from island just by walking
                    Vector3 pos = previousPosition;
                    pos.Y = player.GetVector3("position").Y;
                    player.SetVector3("position", pos);
//                    Console.WriteLine("position reset");
                }
                else
                {
                    // only "leave" island after certain amount of time
                    if (islandLeftAt != 0 /*
                        && at > islandLeftAt + constants.GetFloat("island_leave_timeout")*/)
                    {
//                        Console.WriteLine("island left");
                        ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;
                        activeIsland = null;
                        islandLeftAt = 0;
                    }
                    else
                    {
                        if(islandLeftAt == 0)
                            islandLeftAt = at;
                    }
                }
            }

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
            collisionOccured = false;
            islandCollision = false;
            movedByStick = false;

            // get input
            controllerInput.Update(playerIndex);

            #region movement

            // jetpack
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

            // perform island jump
            if (destinationIsland != null)
            {
                Vector3 islandDir = destinationIsland.GetVector3("position") - playerPosition;

                float yRotation = (float)Math.Atan2(islandDir.X, islandDir.Z);
                Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));

                if (islandDir.Length() < 10 // todo: make this a constant
                    && (Math.Sign(lastIslandDir.X) != Math.Sign(islandDir.X)
                    || Math.Sign(lastIslandDir.Z) != Math.Sign(islandDir.Z)))
                {
                    // oscillation -> stop
                    destinationIsland = null;
                    playerVelocity = Vector3.Zero;
                    islandJumpPerformedAt = at;
                }
                else
                {
                    lastIslandDir = islandDir;

                    islandDir.Normalize();
                    Vector3 velocity = islandDir * constants.GetFloat("island_jump_speed");

                    playerPosition += velocity * dt;
                }
            }

            // gravity
            if (playerVelocity.Length() <= constants.GetFloat("max_gravity_speed")
                || playerVelocity.Y > 0) // gravity max speed only applies for downwards speeds
            {
                playerVelocity += constants.GetVector3("gravity_acceleration") * dt;
            }
            
            // apply current velocity
            playerPosition += playerVelocity * dt;

//            Console.WriteLine();
//            Console.WriteLine("at: " + (int)gameTime.TotalGameTime.TotalMilliseconds);
//            Console.WriteLine("velocity: " + playerVelocity + " led to change from " + previousPosition + " to " + playerPosition);

            if (controllerInput.moveStickMoved)
            {
                movedByStick = true;

                // XZ movement
                if (fuel > 0 && activeIsland == null)
                {
                    // in air
                    playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_jetpack_multiplier");
                    playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_jetpack_multiplier");
                }
                else
                {
                    // on ground
                    playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_movement_multiplier");
                    playerPosition.Z += controllerInput.leftStickY * dt * constants.GetFloat("z_axis_movement_multiplier");
                }

                // rotation
                if (destinationIsland == null)
                {
                    float yRotation = (float)Math.Atan2(controllerInput.leftStickX, controllerInput.leftStickY);
                    Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                    player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));
                }
            }

            // pushback
            Game.ApplyPushback(ref playerPosition, ref collisionPushbackVelocity, 0f);
            Game.ApplyPushback(ref playerPosition, ref playerPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));
            Game.ApplyPushback(ref playerPosition, ref hitPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));

            // frozen!?
            if (player.GetInt("frozen") > 0)
            {
                playerPosition = (previousPosition + playerPosition) / 2;
                player.SetInt("frozen", player.GetInt("frozen") - (int)simTime.DtMs);
                if (player.GetInt("frozen") < 0)
                    player.SetInt("frozen", 0);
            }

            #endregion

            #region actions

            // ice spike
            if (controllerInput.iceSpikeButtonPressed && player.GetInt("energy") > constants.GetInt("ice_spike_energy_cost") 
                && (at - iceSpikeFiredAt) > constants.GetInt("ice_spike_cooldown"))
            {
                // indicate 
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
                soundEffect.Play(Game.Instance.EffectsVolume);

                // todo: specify point in model
                Vector3 pos = new Vector3(playerPosition.X+5, playerPosition.Y+10, playerPosition.Z+5);
                Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), Game.GetRotation(player));

                #region search next player in range

                float angle = constants.GetFloat("ice_spike_aim_angle");
                float aimDistance = float.PositiveInfinity;
                Entity targetPlayer = null;
                Vector3 distVector = Vector3.Zero;
                foreach (Entity p in Game.Instance.Simulation.PlayerManager)
                {
                    if(p != player)
                    {
                        Vector3 pp = p.GetVector3("position");
                        Vector3 pdir = pp - playerPosition;
                        float a = (float) (Math.Acos(Vector3.Dot(pdir, viewVector) / pdir.Length() / viewVector.Length()) / Math.PI * 180);
                        if(a < angle)
                        {
                            float ad = pdir.Length();
                            if(ad < aimDistance)
                            {
                                targetPlayer = p;
                                distVector = pdir;
                                aimDistance = ad;
                            }
                        }
                    }
                }
                String targetPlayerName = targetPlayer!=null ? targetPlayer.Name : "";
                //Console.WriteLine("targetPlayer: " + targetPlayerName);

                #endregion

                Vector3 aimVector = viewVector;
                if(targetPlayer != null)
                {
                    aimVector = Vector3.Normalize(distVector);
                }
                aimVector *= constants.GetFloat("ice_spike_speed");

                Entity iceSpike = new Entity("icespike" + (++iceSpikeCount)+"_"+player.Name);
                iceSpike.AddStringAttribute("player", player.Name);
                iceSpike.AddStringAttribute("target_player", targetPlayerName);
                iceSpike.AddIntAttribute("creation_time", (int) at);

                iceSpike.AddVector3Attribute("velocity", aimVector);
                iceSpike.AddVector3Attribute("position", pos);

                iceSpike.AddStringAttribute("mesh", "Models/icespike_primitive");
                iceSpike.AddVector3Attribute("scale", new Vector3(5, 5, 5));

                iceSpike.AddStringAttribute("bv_type", "sphere");

                iceSpike.AddProperty("render", new RenderProperty());
                iceSpike.AddProperty("collision", new CollisionProperty());
                iceSpike.AddProperty("controller", new IceSpikeControllerProperty());

                Game.Instance.Simulation.EntityManager.AddDeferred(iceSpike);

                // update states
                player.SetInt("energy", player.GetInt("energy") - constants.GetInt("ice_spike_energy_cost"));
                iceSpikeFiredAt = at;
            }

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

                        Vector3 pos = new Vector3(playerPosition.X+10, playerPosition.Y+10, playerPosition.Z);
                        Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), Game.GetRotation(player));

                        flame = new Entity("flame" + "_" + player.Name);
                        flame.AddStringAttribute("player", player.Name);
                        flame.AddBoolAttribute("active", false);

                        flame.AddVector3Attribute("velocity", viewVector);
                        flame.AddVector3Attribute("position", pos);

                        flame.AddStringAttribute("mesh", "Models/flame_primitive");
                        flame.AddVector3Attribute("scale", new Vector3(0, 0, 0));
                        flame.AddVector3Attribute("full_scale", new Vector3(26, 26, 26));
                        flame.AddQuaternionAttribute("rotation", Game.GetRotation(player));

                        flame.AddStringAttribute("bv_type", "sphere");

                        flame.AddProperty("render", new RenderProperty());
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

            // recharge energy
            if (flame == null)
                Game.Instance.ApplyIntervalAddition(player, "energy_recharge", constants.GetInt("energy_recharge_interval"),
                    player.GetIntAttribute("energy"));

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

            // island selection and attraction
            if (attractedIsland == null
                && destinationIsland == null)
            {
                if (controllerInput.rightStickMoved
                    && activeIsland != null)
                {
                    if(selectedIsland == null // only allow reselection after timeout
                        || at > islandSelectedAt + constants.GetFloat("island_deselection_timeout"))
                    {
                        // select closest island in direction of stick
                        Vector3 stickDir = new Vector3(controllerInput.rightStickX, 0, controllerInput.rightStickY);
                        stickDir.Normalize();
                        selectedIsland = selectBestIsland(stickDir);

                        if (selectedIsland != null)
                        {
                            islandSelectedAt = at;
                            arrow.SetString("island", selectedIsland.Name);

                            if (!arrow.HasProperty("render"))
                            {
                                arrow.AddProperty("render", new RenderProperty());
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
                    }
                }

                // island attraction start
                if (controllerInput.attractionButtonPressed
                    && selectedIsland != null)
                {
                    attractedIsland = selectedIsland;
                    attractedIsland.SetString("attracted_by", player.Name);
                }


            }
            else
            {
                // deactivate attraction
                if (attractedIsland != null
                    && !controllerInput.attractionButtonPressed)
                {
                    arrow.RemoveProperty("render");
                    arrow.RemoveProperty("shadow_cast");

                    attractedIsland.SetString("attracted_by", "");
                    attractedIsland = null;
                    selectedIsland = null;
                }
            }

            // island jump start
            if (controllerInput.jetpackButtonPressed
                && selectedIsland != null
                && at > islandJumpPerformedAt + constants.GetFloat("island_jump_interval")
            )
            {
                destinationIsland = selectedIsland;

                // calculate time to travel to island (in xz plane)
                Vector3 islandDir = destinationIsland.GetVector3("position") - playerPosition;
                lastIslandDir = islandDir;
                Vector3 islandDir2D = islandDir;
                islandDir2D.Y = 0;
                float dist2D = islandDir2D.Length();
                float time = dist2D / constants.GetFloat("island_jump_speed");

                if (islandDir.Y > 0)
                // use time to calculate speed on y-axis for nice jump, and multiplie with constant
                // that defines arc
                {
                    playerVelocity =
                        (Vector3.UnitY * islandDir.Y / time // component to travel Y distance
                        - constants.GetVector3("gravity_acceleration") * time) // component to beat gravity
                        * constants.GetFloat("island_jump_height_multiplier"); // modifier for arc
                }
                else
                {
                    playerVelocity = -constants.GetVector3("gravity_acceleration") * time // component against gravity
                            * constants.GetFloat("island_jump_height_multiplier"); // modifier for arc
                }
            }

            #endregion

            // recharge
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

            // update player attributes
            player.SetInt("fuel", fuel);

            player.SetVector3("position", playerPosition);
            player.SetVector3("velocity", playerVelocity);
            player.SetVector3("collision_pushback_velocity", collisionPushbackVelocity);
            player.SetVector3("player_pushback_velocity", playerPushbackVelocity);
            player.SetVector3("hit_pushback_velocity", hitPushbackVelocity);

            CheckPlayerAttributeRanges(player);

            // check collision with lava
            Entity lava = Game.Instance.Simulation.EntityManager["lava"];
            if (playerPosition.Y < lava.GetVector3("position").Y)
                PlayerLavaCollisionHandler(simTime, player, lava);
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
                    case "powerup":
                        PlayerPowerupCollisionHandler(simTime, contact.EntityA, contact.EntityB);
                        break;
                }
                CheckPlayerAttributeRanges(player);
                collisionOccured = true;
            }
        }

        private void PlayerIslandCollisionHandler(SimulationTime simTime, Entity player, Entity island, Contact contact)
        {
            if (island == destinationIsland)
            {
                // stop island jump
                destinationIsland = null;
                islandJumpPerformedAt = simTime.At;
                return;
            }

            // todo: this of course has to be checkd in island - island collision
            // use listener on attracted_by attribute to do things in player if deactivated attraction
            if (island == destinationIsland)
            {
                // stop island attraction
                arrow.RemoveProperty("render");
                arrow.RemoveProperty("shadow_cast");

                attractedIsland.SetString("attracted_by", "");
                attractedIsland = null;
                selectedIsland = null;                 
                return;
            }

            float dt = simTime.Dt;

            // on top?
            if (Vector3.Dot(Vector3.UnitY, contact[0].Normal) < 0
                && destinationIsland == null // no on top while jumping...
            )
            {
//                Console.WriteLine("on island " + island.Name); 

                // add handler if active island changed
                if (activeIsland != island)
                {
  //                  Console.WriteLine((int)gameTime.TotalGameTime.TotalMilliseconds + island.Name + " activated");
                    ((Vector3Attribute)island.Attributes["position"]).ValueChanged += IslandPositionHandler;

                    // remove old handler if there was other island active before
                    if (activeIsland != null)
                    {
//                        Console.WriteLine("island changed from " + activeIsland.Name);
                        ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;
                    }

                    activeIsland = island;
                }

                // stop falling
                player.SetVector3("velocity", Vector3.Zero);

                // determine contact to use (the one with highest y value)
                Vector3 point = contact[0].Point;
                for (int i = 1; i < contact.Count; i++)
                {
                    if (contact[i].Point.Y > contact[i].Point.Y)
                        { point = contact[i].Point; }
                }

                // set position to contact point
                Vector3 pos = player.GetVector3("position");
                pos.Y = point.Y;
                player.SetVector3("position", pos);

//                Console.WriteLine("player positioned at: " + pos);

                // and set state values
                islandCollision = true;
                activeIsland = island; // mark as active
            }
            else
            {
                Vector3 pos = player.GetVector3("position");
                // todo constant 2??
                Vector3 velocity = -contact[0].Normal * (pos - previousPosition).Length() * simTime.Dt / 2;
                player.SetVector3("collision_pushback_velocity", velocity);
            }
        }

        private void PlayerPillarCollisionHandler(SimulationTime simTime, Entity player, Entity pillar, Contact co)
        {
            Vector3 pos = player.GetVector3("position");
            Vector3 velocity = -co[0].Normal * (pos - previousPosition).Length() * simTime.Dt;
            player.SetVector3("collision_pushback_velocity", velocity);
        }

        private void PlayerLavaCollisionHandler(SimulationTime simTime, Entity player, Entity lava)
        {
            Game.Instance.ApplyPerSecondSubstraction(player, "lava_damage", constants.GetInt("lava_damage_per_second"),
                player.GetIntAttribute("health"));
        }

        private void PlayerPowerupCollisionHandler(SimulationTime simTime, Entity player, Entity powerup)
        {
            // remove 
            powerup.RemoveProperty("collision");
            powerup.RemoveProperty("render");
            powerup.RemoveProperty("shadow_cast");

            // set respawn
            // todo: extract constants into xml
            powerup.SetFloat("respawn_at", (float) (simTime.At + rand.NextDouble() * 10000 + 5000));

            // use the power
            int oldVal = player.GetInt(powerup.GetString("power"));
            oldVal += powerup.GetInt("powerValue");
            player.SetInt(powerup.GetString("power"), oldVal);
            
            // soundeffect
            SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/" + powerup.GetString("pickup_sound"));
            soundEffect.Play(Game.Instance.EffectsVolume);
        }

        private void PlayerPlayerCollisionHandler(SimulationTime simTime, Entity player, Entity otherPlayer, Contact c)
        {
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
                if(movedByStick) // apply feedback to player that moved into the other one
                    player.SetVector3("player_pushback_velocity", player.GetVector3("player_pushback_velocity")
                        -c[0].Normal * constants.GetFloat("player_pushback_velocity_multiplier"));
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
        private Entity selectBestIsland(Vector3 dir)
        {
            float nearestDist = float.MaxValue;
            Entity selectedIsland = null;
            foreach (Entity island in Game.Instance.Simulation.IslandManager)
            {
                Vector3 islandDir = island.GetVector3("position") - player.GetVector3("position");
                float dist = islandDir.Length();
                islandDir.Y = 0;
                float angle = (float)(Math.Acos(Vector3.Dot(dir, islandDir) / dist) / Math.PI * 180);
                if (angle < constants.GetFloat("island_aim_angle")
                    && dist < nearestDist
                    && island != activeIsland)
                {
                    selectedIsland = island;
                    nearestDist = dist;
                }
            }

            return selectedIsland;
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
                            leftStickY = -gamepadEmulationValue;
                            moveStickMoved = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.S))
                            {
                                leftStickY = gamepadEmulationValue;
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
