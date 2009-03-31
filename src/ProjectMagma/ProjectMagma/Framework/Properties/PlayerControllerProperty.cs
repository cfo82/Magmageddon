using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using ProjectMagma.Collision;
using ProjectMagma.Collision.CollisionTests;

namespace ProjectMagma.Framework
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

        private double islandRepulsionStarteAt = 0;
        private double islandRepulsionStoppedAt = 0;

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

            this.constants = Game.Instance.EntityManager["player_constants"];

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

            Game.Instance.EntityManager.EntityRemoved += EntityRemovedHandler;
            ((CollisionProperty)player.GetProperty("collision")).OnContact += PlayerCollisionHandler;

            jetpackSound = Game.Instance.Content.Load<SoundEffect>("Sounds/jetpack");
            flameThrowerSound = Game.Instance.Content.Load<SoundEffect>("Sounds/flamethrower");

            arrow = new Entity("arrow" + "_" + player.Name);
            arrow.AddStringAttribute("player", player.Name);

            arrow.AddVector3Attribute("position", Vector3.Zero);
            arrow.AddStringAttribute("island", "");

            arrow.AddStringAttribute("mesh", "Models/arrow");
            arrow.AddVector3Attribute("scale", new Vector3(12, 12, 12));

            arrow.AddProperty("arrow_controller_property", new ArrowController()); 

            Game.Instance.EntityManager.AddDeferred(arrow);

            this.previousPosition = player.GetVector3("position");
            this.player = player;
        }

        public void OnDetached(Entity player)
        {
            player.Update -= OnUpdate;
            Game.Instance.EntityManager.Remove(arrow);
            if(flame != null)
                Game.Instance.EntityManager.Remove(flame);
            Game.Instance.EntityManager.EntityRemoved -= EntityRemovedHandler;
            ((CollisionProperty)player.GetProperty("collision")).OnContact -= PlayerCollisionHandler;
        }

        private void OnUpdate(Entity player, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;
            double at = gameTime.TotalGameTime.TotalMilliseconds;

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

                    Game.Instance.Content.Load<SoundEffect>("Sounds/death").Play(Game.Instance.EffectsVolume);
                    player.SetInt("deaths", player.GetInt("deaths") + 1);

                    player.RemoveProperty("render");
                    player.RemoveProperty("shadow_cast");
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
                        int islandNo = rand.Next(Game.Instance.IslandManager.Count - 1);
                        Entity island = Game.Instance.IslandManager[islandNo];
                        Vector3 pos = island.GetVector3("position");
                        pos.Y = pos.Y + 30; // todo: change this to point defined in mesh
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
                    player.SetVector3("position", previousPosition);
                }
                else
                {
                    ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;
                    activeIsland = null;
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
            if (controllerInput.jetpackButtonPressed && fuel > 0 && flame == null)
            {
                if (!jetpackActive)
                {
                    jetpackSoundInstance = jetpackSound.Play(0.4f * Game.Instance.EffectsVolume, 1, 0, true);
                    jetpackActive = true;
                }
                
                fuel -= gameTime.ElapsedGameTime.Milliseconds;
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

            // gravity
            if (playerVelocity.Length() <= constants.GetFloat("max_gravity_speed"))
                playerVelocity += constants.GetVector3("gravity_acceleration") * dt;
            
            // apply current velocity
            playerPosition += playerVelocity * dt;

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
                float yRotation = (float)Math.Atan2(controllerInput.leftStickX, -controllerInput.leftStickY);
                Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));
            }

            // pushback
            Game.ApplyPushback(ref playerPosition, ref collisionPushbackVelocity, 0f);
            Game.ApplyPushback(ref playerPosition, ref playerPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));
            Game.ApplyPushback(ref playerPosition, ref hitPushbackVelocity, constants.GetFloat("player_pushback_deacceleration"));

            // frozen!?
            if (player.GetInt("frozen") > 0)
            {
                playerPosition = (previousPosition + playerPosition) / 2;
                player.SetInt("frozen", player.GetInt("frozen") - gameTime.ElapsedGameTime.Milliseconds);
                if (player.GetInt("frozen") < 0)
                    player.SetInt("frozen", 0);
            }

            #endregion

            #region actions

            // ice spike
            if (controllerInput.iceSpikeButtonPressed && player.GetInt("energy") > constants.GetInt("ice_spike_energy_cost") &&
                (at - iceSpikeFiredAt) > constants.GetInt("ice_spike_cooldown"))
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
                foreach(Entity p in Game.Instance.PlayerManager)
                {
                    if(p != player)
                    {
                        Vector3 pp = p.GetVector3("position");
                        Vector3 pdir = pp - playerPosition;
                        float a = (float) (Math.Acos(Vector3.Dot(pdir, viewVector) / pdir.Length() / viewVector.Length()) / Math.PI * 360);
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

                float strength = 0.6f+controllerInput.iceSpikeStrength;
                //aimVector.Normalize();
                //aimVector *= constants.GetFloat("ice_spike_speed") * strength;
                //aimVector.Y = constants.GetFloat("ice_spike_up_speed");
                Vector3 aimVector = viewVector;
                if(targetPlayer != null)
                {
                    aimVector.Y = Vector3.Normalize(distVector).Y;
                    //aimVector = Vector3.Normalize(distVector);
                }
                aimVector *= constants.GetFloat("ice_spike_speed") * strength;

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

                Game.Instance.EntityManager.AddDeferred(iceSpike);

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

                        Game.Instance.EntityManager.AddDeferred(flame);
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
            if (controllerInput.rightStickMoved
                && activeIsland != null
                && fuel > 0)
            {
                if (at > islandRepulsionStoppedAt + constants.GetFloat("island_repulsion_interval_time"))
                {
                    if (islandRepulsionStarteAt == 0
                        && player.GetInt("fuel") > constants.GetInt("island_repulsion_min_fuel"))
                        islandRepulsionStarteAt = at;
                    if (at < islandRepulsionStarteAt + constants.GetFloat("island_repulsion_max_time"))
                    {
                        // TODO: constant
                        float velocityMultiplier = constants.GetFloat("island_repulsion_velocity_multiplier");
                        Vector3 velocity = new Vector3(-controllerInput.rightStickX * velocityMultiplier, 0, 
                            -controllerInput.rightStickY * velocityMultiplier);
                        activeIsland.SetVector3("repulsion_velocity", activeIsland.GetVector3("repulsion_velocity") + velocity);

                        fuel -= (int) (gameTime.ElapsedGameTime.Milliseconds * velocity.Length() * 
                            constants.GetFloat("island_repulsion_fuel_cost_multiplier"));
                    }
                    else
                    {
                        islandRepulsionStarteAt = 0;
                        islandRepulsionStoppedAt = at;
                    }
                }
            }
            else
            {
                if(islandRepulsionStarteAt > 0)
                {
                    islandRepulsionStarteAt = 0;
                    islandRepulsionStoppedAt = at;
                }
            }

            // island attraction
            if (!arrow.HasProperty("render"))
            {
                int islandNo = rand.Next(Game.Instance.IslandManager.Count - 1);
                Entity island = Game.Instance.IslandManager[islandNo];

                arrow.SetString("island", island.Name);

                arrow.AddProperty("render", new RenderProperty());
                arrow.AddProperty("shadow_cast", new ShadowCastProperty());
            }

            #endregion

            // recharge
            if (!controllerInput.jetpackButtonPressed)
            {
                if (activeIsland == null)
                {
                    fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier"));
                }
                else
                {
                    // faster recharge standing on island, but only if jetpack was not used for repulsion
                    if (at > islandRepulsionStoppedAt + constants.GetFloat("island_repulsion_recharge_delay"))
                    {
                        fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier_island"));
                    }
                    else
                    {
                        double diff = at - islandRepulsionStoppedAt;
                        fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier_island")
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
            Entity lava = Game.Instance.EntityManager["lava"];
            if (playerPosition.Y < lava.GetVector3("position").Y)
                PlayerLavaCollisionHandler(gameTime, player, lava);
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

        private void PlayerCollisionHandler(GameTime gameTime, List<Contact> contacts)
        {
            // average contacts
            Vector3 position = Vector3.One;
            Vector3 normal = Vector3.One;
            foreach(Contact co in contacts)
            {
                position += co.Position;
                normal += co.Normal;
            }
            position /= contacts.Count;
            normal /= contacts.Count;
//            Contact c = new Contact(contacts[0].EntityA, contacts[0].EntityB, position, normal);
            Contact c = contacts[0];

            if (c.EntityB.HasAttribute("kind"))
            {
                String kind = c.EntityB.GetString("kind");
                switch (kind)
                {
                    case "island":
                        PlayerIslandCollisionHandler(gameTime, c.EntityA, c.EntityB, c);
                        break;
                    case "pillar":
                        PlayerPillarCollisionHandler(gameTime, c.EntityA, c.EntityB, c);
                        break;
                    case "player":
                        PlayerPlayerCollisionHandler(gameTime, c.EntityA, c.EntityB, c);
                        break;
                    case "powerup":
                        PlayerPowerupCollisionHandler(gameTime, c.EntityA, c.EntityB);
                        break;
                }
                CheckPlayerAttributeRanges(player);
                collisionOccured = true;
            }
        }

        private void PlayerIslandCollisionHandler(GameTime gameTime, Entity player, Entity island, Contact co)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds) / 1000.0f;

            if (Vector3.Dot(Vector3.UnitY, -co.Normal) > 0)
            {
                // standing on island
                //Console.WriteLine("from top at "+gameTime.TotalGameTime.TotalMilliseconds);

                // remove handler from old active island
                if(activeIsland != null && activeIsland != island)
                    ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;

                // add handler if active island changed
                if (activeIsland != island)
                    ((Vector3Attribute)island.Attributes["position"]).ValueChanged += IslandPositionHandler;

                Vector3 pos = player.GetVector3("position");
                /*
                pos.Y = co.Position.Y;
                player.SetVector3("position", pos);
                 */

                Vector3 velocity = new Vector3(0, -(pos.Y - co.Position.Y), 0) * 1000 / gameTime.ElapsedGameTime.Milliseconds;
                player.SetVector3("collision_pushback_velocity", velocity);

                player.SetVector3("velocity", Vector3.Zero);
                islandCollision = true;
                activeIsland = island; // mark as active
            }
            else
            {
                Vector3 pos = player.GetVector3("position");
                Vector3 velocity = -co.Normal * (pos - previousPosition).Length() * 1000 / gameTime.ElapsedGameTime.Milliseconds 
                    / 2;
                player.SetVector3("collision_pushback_velocity", velocity);
            }
        }

        private void PlayerPillarCollisionHandler(GameTime gameTime, Entity player, Entity pillar, Contact co)
        {
            if (co.Normal.Y < 0)
            {
                // top
                player.SetVector3("velocity", Vector3.Zero);

                Vector3 pos = player.GetVector3("position");
                pos.Y = previousPosition.Y;
                player.SetVector3("position", pos);
            }
            else
            {
                Vector3 pos = player.GetVector3("position");
                Vector3 velocity = -co.Normal * (pos-previousPosition).Length() * 1000 / gameTime.ElapsedGameTime.Milliseconds;
                player.SetVector3("collision_pushback_velocity", velocity);
            }
        }

        private void PlayerLavaCollisionHandler(GameTime gameTime, Entity player, Entity lava)
        {
            Game.Instance.ApplyPerSecondSubstraction(player, "lava_damage", constants.GetInt("lava_damage_per_second"),
                player.GetIntAttribute("health"));
        }

        private void PlayerPowerupCollisionHandler(GameTime gameTime, Entity player, Entity powerup)
        {
            // remove 
            powerup.RemoveProperty("collision");
            powerup.RemoveProperty("render");
            powerup.RemoveProperty("shadow_cast");

            // set respawn
            powerup.SetFloat("respawn_at", (float) (gameTime.TotalGameTime.TotalMilliseconds + rand.NextDouble()
                * 10000 + 5000));

            // use the power
            int oldVal = player.GetInt(powerup.GetString("power"));
            oldVal += powerup.GetInt("powerValue");
            player.SetInt(powerup.GetString("power"), oldVal);
            
            // soundeffect
            SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/" + powerup.GetString("pickup_sound"));
            soundEffect.Play(Game.Instance.EffectsVolume);
        }

        private void PlayerPlayerCollisionHandler(GameTime gameTime, Entity player, Entity otherPlayer, Contact c)
        {
            // and hit?
            if (controllerInput.hitButtonPressed &&
                gameTime.TotalGameTime.TotalMilliseconds > hitPerformedAt + constants.GetInt("hit_cooldown"))
            {
                // indicate hit!
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
                soundEffect.Play(Game.Instance.EffectsVolume);

                // deduct health
                otherPlayer.SetInt("health", otherPlayer.GetInt("health") - constants.GetInt("hit_damage"));
                CheckPlayerAttributeRanges(otherPlayer);

                // set values
                otherPlayer.SetVector3("hit_pushback_velocity", c.Normal * constants.GetFloat("hit_pushback_velocity_multiplier"));
                hitPerformedAt = gameTime.TotalGameTime.TotalMilliseconds;
            }
            else
            {
                // normal feedback
                player.SetVector3("player_pushback_velocity", -c.Normal * constants.GetFloat("player_pushback_velocity_multiplier") / 2);
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

                #region triggers
                iceSpikeStrength = gamePadState.Triggers.Right;
                #endregion


                #region action buttons

                jetpackButtonPressed =
                    gamePadState.Triggers.Left > 0 ||
                    (keyboardState.IsKeyDown(Keys.Space) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Insert) && playerIndex == PlayerIndex.Two);

                iceSpikeButtonPressed = false;
                if (iceSpikeStrength > 0)
                    iceSpikeButtonPressed = true;
                else
                if((keyboardState.IsKeyDown(Keys.Q) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.RightControl) && playerIndex == PlayerIndex.Two))
                {
                    iceSpikeButtonPressed = true;
                    iceSpikeStrength = 1;
                }

                hitButtonPressed =
                    gamePadState.Buttons.RightShoulder == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.E) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Enter) && playerIndex == PlayerIndex.Two);

                flamethrowerButtonPressed = 
                    gamePadState.Buttons.A == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.R) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Delete) && playerIndex == PlayerIndex.Two);

                #endregion

            }

            // in order to use the following variables as private with getters/setters, do
            // we really need 15 lines per variable?!
            
            // joystick
            public float leftStickX, leftStickY;
            public bool moveStickMoved;
            public float rightStickX, rightStickY;
            public bool rightStickMoved;

            // triggers
            public float iceSpikeStrength;

            // buttons
            public bool jetpackButtonPressed, flamethrowerButtonPressed, iceSpikeButtonPressed, hitButtonPressed;

            private static float gamepadEmulationValue = -1f;
        }

        ControllerInput controllerInput;
    }


}
