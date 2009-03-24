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
using ProjectMagma.Shared.BoundingVolume;
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

        private double energyRechargedAt = 0;

        private int iceSpikeCount = 0;
        private int iceSpikeRemovedCount = 0;
        private double iceSpikeFiredAt = 0;

        private double hitPerformedAt = 0;

        Entity flame = null;

        private double lavaContactAt = 0;

        private SoundEffect jetpackSound;
        private SoundEffectInstance jetpackSoundInstance;
        private SoundEffect flameThrowerSound;
        private SoundEffectInstance flameThrowerSoundInstance;

        Vector3 originalPosition;

        public PlayerControllerProperty()
        {
        }

        public void OnAttached(Entity player)
        {
            player.Update += OnUpdate;

            this.constants = Game.Instance.EntityManager["player_constants"];

            player.AddQuaternionAttribute("rotation", Quaternion.Identity);
            player.AddVector3Attribute("velocity", Vector3.Zero);

            player.AddVector3Attribute("contact_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddIntAttribute("energy", constants.GetInt("max_energy"));
            player.AddIntAttribute("health", constants.GetInt("max_health"));
            player.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            player.AddIntAttribute("kills", 0);
            player.AddIntAttribute("deaths", 0);

            player.AddIntAttribute("frozen", 0);
            player.AddStringAttribute("collisionPlayer", "");

            Game.Instance.EntityManager.EntityRemoved += new EntityRemovedHandler(EntityRemovedHandler);
            ((CollisionProperty)player.GetProperty("collision")).OnContact += new ContactHandler(PlayerCollisionHandler);

            jetpackSound = Game.Instance.Content.Load<SoundEffect>("Sounds/jetpack");
            flameThrowerSound = Game.Instance.Content.Load<SoundEffect>("Sounds/flamethrower");

            this.player = player;
        }

        public void OnDetached(Entity player)
        {
            player.Update -= OnUpdate;
            Game.Instance.EntityManager.EntityRemoved -= new EntityRemovedHandler(EntityRemovedHandler);
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

                    Game.Instance.Content.Load<SoundEffect>("Sounds/death").Play();
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

                        int islandNo = rand.Next(Game.Instance.IslandManager.Count - 1);
                        Entity island = Game.Instance.IslandManager[islandNo];
                        BoundingBox bb = Game.CalculateBoundingBox(island);
                        Vector3 pos = island.GetVector3("position");
                        pos.Y = bb.Max.Y + 10;
                        player.SetVector3("position", pos);

                        // reset
                        player.SetQuaternion("rotation", Quaternion.Identity);
                        player.SetVector3("velocity", Vector3.Zero);

                        player.SetVector3("contact_pushback_velocity", Vector3.Zero);
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

            // island leave check
            if (islandCollision == false && activeIsland != null)
            {
                ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;
                activeIsland = null;
            }
            else
                islandCollision = false;

            PlayerIndex playerIndex = (PlayerIndex)player.GetInt("game_pad_index");
            Vector3 playerPosition = player.GetVector3("position");
            Vector3 playerVelocity = player.GetVector3("velocity");
            Vector3 contactPushbackVelocity = player.GetVector3("contact_pushback_velocity");
            Vector3 hitPushbackVelocity = player.GetVector3("hit_pushback_velocity");
            
            int fuel = player.GetInt("fuel");

            Model playerModel = Game.Instance.Content.Load<Model>(player.GetString("mesh"));

            originalPosition = playerPosition;

            // get input
            controllerInput.Update(playerIndex);

            #region movement

            // jetpack
            if (controllerInput.jetpackButtonPressed && fuel > 0 && flame == null)
            {
                if (!jetpackActive)
                {
                    jetpackSoundInstance = jetpackSound.Play(0.4f, 1, 0, true);
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
                if (!controllerInput.jetpackButtonPressed)
                {
                    if (activeIsland == null)
                    {
                        fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier"));
                    }
                    else
                    {
                        // faster recharge standing on island
                        fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier_island"));
                    }
                }
            }

            // gravity
            if (playerVelocity.Length() <= constants.GetFloat("max_gravity_speed"))
                playerVelocity += constants.GetVector3("gravity_acceleration") * dt;
            
            playerPosition += playerVelocity * dt;

            // XZ movement
            if (fuel > 0 && activeIsland == null)
            {
                // in air
                playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_jetpack_multiplier");
                playerPosition.Z -= controllerInput.leftStickY * dt * constants.GetFloat("z_axis_jetpack_multiplier");
            }
            else
            {
                // on ground
                playerPosition.X += controllerInput.leftStickX * dt * constants.GetFloat("x_axis_movement_multiplier");
                playerPosition.Z -= controllerInput.leftStickY * dt * constants.GetFloat("z_axis_movement_multiplier");

                // prevent the player from walking down the island
                if (activeIsland != null && !controllerInput.jetpackButtonPressed)
                {
                    // TODO: how to do this in future?
                    BoundingCylinder ibc = Game.CalculateBoundingCylinder(activeIsland);

                    Vector2 pp = new Vector2(playerPosition.X, playerPosition.Z);
                    Vector2 ic = new Vector2(ibc.Top.X, ibc.Bottom.Z);
                    Vector2 diff = ic - pp;
                    if (diff.Length() > ibc.Radius)
                    {
                        Vector2 op = new Vector2(originalPosition.X, originalPosition.Z);
                        if ((op - ic).Length() < diff.Length())
                        {
                            playerPosition.X = originalPosition.X;
                            playerPosition.Z = originalPosition.Z;
                        }
                    }
                }
            }

            // rotation
            if (controllerInput.moveStickPressed)
            {
                float yRotation = (float)Math.Atan2(controllerInput.leftStickX, -controllerInput.leftStickY);
                Matrix rotationMatrix = Matrix.CreateRotationY(yRotation);
                player.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));
            }

            // frozen!?
            if (player.GetInt("frozen") > 0)
            {
                playerPosition = (originalPosition + playerPosition) / 2;
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
                soundEffect.Play();

                BoundingBox bb = Game.CalculateBoundingBox(player);
                Vector3 pos = new Vector3(playerPosition.X, bb.Max.Y, playerPosition.Z);
                Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), Game.GetRotation(player));

                // search next player in range
                float angle = constants.GetFloat("ice_spike_aim_angle");
                Vector3 aimVector = viewVector;
                float aimDistance = float.PositiveInfinity;
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
                                aimVector = pdir;
                                aimDistance = ad;
                            }
                        }
                    }
                }

                float strength = 0.6f+controllerInput.iceSpikeStrength;
                aimVector.Normalize();
                aimVector *= constants.GetFloat("ice_spike_speed") * strength;
                aimVector.Y = constants.GetFloat("ice_spike_up_speed");

                Entity iceSpike = new Entity(Game.Instance.EntityManager, "icespike" + (++iceSpikeCount)+"_"+player.Name);
                iceSpike.AddStringAttribute("player", player.Name);

                iceSpike.AddVector3Attribute("velocity", aimVector);
                iceSpike.AddVector3Attribute("position", pos);

                iceSpike.AddStringAttribute("mesh", "Models/icespike_primitive");
                iceSpike.AddVector3Attribute("scale", new Vector3(5, 5, 5));

                iceSpike.AddProperty("render", new RenderProperty());
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
                        flameThrowerSoundInstance = flameThrowerSound.Play(1, 1, 0, true);

                        BoundingBox bb = Game.CalculateBoundingBox(player);
                        Vector3 pos = new Vector3(bb.Max.X, bb.Max.Y, playerPosition.Z);
                        Vector3 viewVector = Vector3.Transform(new Vector3(0, 0, 1), Game.GetRotation(player));

                        flame = new Entity(Game.Instance.EntityManager, "flame" + "_" + player.Name);
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

            // pushback
            if (contactPushbackVelocity.Length() > 0)
            {
                Vector3 oldVelocity = contactPushbackVelocity;
                Vector3 pushbackDeAcceleration = contactPushbackVelocity;
                pushbackDeAcceleration.Normalize();

                contactPushbackVelocity -= pushbackDeAcceleration * constants.GetFloat("pushback_deacceleration_multiplier") * dt;
                if (contactPushbackVelocity.Length() > oldVelocity.Length()) // if length increases we accelerate -> stop
                    contactPushbackVelocity = Vector3.Zero;

                playerPosition += contactPushbackVelocity * dt;
            }
            if (hitPushbackVelocity.Length() > 0)
            {
                Vector3 oldVelocity = hitPushbackVelocity;
                Vector3 pushbackDeAcceleration = hitPushbackVelocity;
                pushbackDeAcceleration.Normalize();

                hitPushbackVelocity -= pushbackDeAcceleration * constants.GetFloat("pushback_deacceleration_multiplier") * dt;
                if (hitPushbackVelocity.Length() > oldVelocity.Length()) // if length increases we accelerate -> stop
                    hitPushbackVelocity = Vector3.Zero;

                playerPosition += hitPushbackVelocity * dt;
            }

            // recharge energy
            if ((gameTime.TotalGameTime.TotalMilliseconds - energyRechargedAt) > constants.GetInt("energy_recharge_interval")
                && flame == null)
            {
                player.SetInt("energy", player.GetInt("energy") + 1);
                energyRechargedAt = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            #endregion

            // update player attributes
            player.SetInt("fuel", fuel);

            player.SetVector3("position", playerPosition);
            player.SetVector3("velocity", playerVelocity);
            player.SetVector3("contact_pushback_velocity", contactPushbackVelocity);
            player.SetVector3("hit_pushback_velocity", hitPushbackVelocity);

            CheckPlayerAttributeRanges(player);

            /// TODO: move this to collision manager
            /// collision detection code

            // get bounding sphere
            BoundingSphere bs = Game.CalculateBoundingSphere(player);

            // check collision with juicy powerups
            foreach (Entity powerup in Game.Instance.PowerupManager)
            {
                BoundingBox bb = Game.CalculateBoundingBox(powerup);

                if (bb.Intersects(bs))
                {
                    PlayerPowerupCollisionHandler(gameTime, player, powerup);
                }
            }

            // check collision with lava
            Entity lava = Game.Instance.EntityManager["lava"];
            if (playerPosition.Y < lava.GetVector3("position").Y)
                PlayerLavaCollisionHandler(gameTime, player, lava);
            else // hack hack: need better way to reset perSecond counters
                lavaContactAt = at;
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

        private void PlayerCollisionHandler(GameTime gameTime, Contact c)
        {
            if (c.EntityB.HasAttribute("kind"))
            {
                String kind = c.EntityB.GetString("kind");
                if (kind == "island")
                    PlayerIslandCollisionHandler(gameTime, c.EntityA, c.EntityB, c);
                else
                    if (kind == "pillar")
                        PlayerPillarCollisionHandler(gameTime, c.EntityA, c.EntityB, c);
                    else
                        if (kind == "player")
                            PlayerPlayerCollisionHandler(gameTime, c.EntityA, c.EntityB, c);
                CheckPlayerAttributeRanges(player);
            }
        }

        private void PlayerIslandCollisionHandler(GameTime gameTime, Entity player, Entity island, Contact c)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds) / 1000.0f;
            Vector3 playerPosition = player.GetVector3("position");

            if (c.Normal.Y < 0)
            {
                // standing on island

                // remove handler from old active island
                if(activeIsland != null && activeIsland != island)
                    ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= IslandPositionHandler;

                // correct position to exact touching point
                playerPosition.Y = c.Position.Y;
                // add handler if active island changed
                if (activeIsland != island)
                    ((Vector3Attribute)island.Attributes["position"]).ValueChanged += IslandPositionHandler;

                player.SetVector3("velocity", Vector3.Zero);
                islandCollision = true;
                activeIsland = island; // mark as active
            }
            else
            {
                // hack hack hack
                if (c.Normal.Y == 0)
                {
                    // xz

                // calculate theoretical velocity
                Vector3 velocity = (originalPosition - playerPosition) / dt;

                    /*
                    velocity.Y = 0;
                    player.SetVector3("contact_pushback_velocity", -velocity);
                     */
                }
                else
                {
                   /*
                    Vector3 velocity = player.GetVector3("velocity");
                    player.SetVector3("velocity", -velocity);
                    */
                }
//                Vector3 velocity = player.GetVector3("velocity");
                //if(
//                player.SetVector3("pushback_velocity", velocity - c.normal * velocity.Length());
            }
                /*
                player.SetVector3("velocity", Vector3.Reflect(player.GetVector3("velocity"), -c.normal));
                 */

            player.SetVector3("position", playerPosition);
        }

        private void PlayerPillarCollisionHandler(GameTime gameTime, Entity player, Entity pillar, Contact co)
        {
            Vector3 newPos = originalPosition;
            newPos.Y = player.GetVector3("position").Y;
            player.SetVector3("position", newPos);
        }

        private void PlayerLavaCollisionHandler(GameTime gameTime, Entity player, Entity lava)
        {
            int health = player.GetInt("health");
            lavaContactAt = Game.ApplyPerSecond(gameTime.TotalGameTime.TotalMilliseconds, lavaContactAt, constants.GetInt("lava_damage_per_second"),
                ref health);
            player.SetInt("health", health);
        }

        private void PlayerPowerupCollisionHandler(GameTime gameTime, Entity player, Entity powerup)
        {
            Game.Instance.EntityManager.RemoveDeferred(powerup);

            // use the power
            int oldVal = player.GetInt(powerup.GetString("power"));
            oldVal += powerup.GetInt("powerValue");
            player.SetInt(powerup.GetString("power"), oldVal);

            // soundeffect
            SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/" + powerup.GetString("pickup_sound"));
            soundEffect.Play();
        }

        private void PlayerPlayerCollisionHandler(GameTime gameTime, Entity player, Entity otherPlayer, Contact c)
        {
            Vector3 playerPosition = player.GetVector3("position");

            /*
            BoundingSphere bs = Game.CalculateBoundingSphere(player);
            BoundingSphere obs = Game.CalculateBoundingSphere(otherPlayer);

            if (obs.Intersects(bs))
                otherPlayer.SetString("collisionPlayer", player.Name); // indicate collision
            else
                otherPlayer.SetString("collisionPlayer", ""); // reset collision
             */ 

            // and hit?
            if (controllerInput.hitButtonPressed &&
                (gameTime.TotalGameTime.TotalMilliseconds - hitPerformedAt) > constants.GetInt("hit_cooldown"))
            {
                // indicate hit!
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
                soundEffect.Play();

                // deduct health
                otherPlayer.SetInt("health", otherPlayer.GetInt("health") - constants.GetInt("hit_damage"));

                // set values
                otherPlayer.SetVector3("hit_pushback_velocity", c.Normal * constants.GetFloat("hit_pushback_velocity_multiplier"));
                hitPerformedAt = gameTime.TotalGameTime.TotalMilliseconds;
            }
            else
            {
                // normal feedback
                player.SetVector3("contact_pushback_velocity", -c.Normal * constants.GetFloat("pushback_contact_velocity_multiplier") / 2);
            }
        }

        private void IslandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = player.GetVector3("position");
            Vector3 delta = newValue - oldValue;
            position += delta;
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
                leftStickY = gamePadState.ThumbSticks.Left.Y;
                moveStickPressed = leftStickX != 0.0f || leftStickY != 0.0f;

                if (!moveStickPressed)
                {
                    if (playerIndex == PlayerIndex.One)
                    {
                        if (keyboardState.IsKeyDown(Keys.A))
                        {
                            leftStickX = gamepadEmulationValue;
                            moveStickPressed = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.D))
                            {
                                leftStickX = -gamepadEmulationValue;
                                moveStickPressed = true;
                            }

                        if (keyboardState.IsKeyDown(Keys.W))
                        {
                            leftStickY = -gamepadEmulationValue;
                            moveStickPressed = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.S))
                            {
                                leftStickY = gamepadEmulationValue;
                                moveStickPressed = true;
                            }
                    }
                    else
                    {
                        if (keyboardState.IsKeyDown(Keys.Left))
                        {
                            leftStickX = gamepadEmulationValue;
                            moveStickPressed = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.Right))
                            {
                                leftStickX = -gamepadEmulationValue;
                                moveStickPressed = true;
                            }

                        if (keyboardState.IsKeyDown(Keys.Up))
                        {
                            leftStickY = -gamepadEmulationValue;
                            moveStickPressed = true;
                        }
                        else
                            if (keyboardState.IsKeyDown(Keys.Down))
                            {
                                leftStickY = gamepadEmulationValue;
                                moveStickPressed = true;
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
            
            // joysticks
            public float leftStickX, leftStickY;
            public bool moveStickPressed;

            // triggers
            public float iceSpikeStrength;

            // buttons
            public bool jetpackButtonPressed, flamethrowerButtonPressed, iceSpikeButtonPressed, hitButtonPressed;

            private static float gamepadEmulationValue = -1f;
        }

        ControllerInput controllerInput;
    }


}
