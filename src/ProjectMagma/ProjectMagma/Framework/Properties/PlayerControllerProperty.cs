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

namespace ProjectMagma.Framework
{
    public class PlayerControllerProperty : Property
    {
        private SoundEffect jetpackSound;
        private double jetpackSoundEnd = 0;

        public PlayerControllerProperty()
        {
        }

        public void OnAttached(Entity player)
        {
            player.Update += OnUpdate;

            this.constants = Game.Instance.EntityManager["player_constants"];

            player.AddQuaternionAttribute("rotation", Quaternion.Identity);
            player.AddVector3Attribute("jetpack_velocity", Vector3.Zero);

            player.AddVector3Attribute("contact_pushback_velocity", Vector3.Zero);
            player.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            player.AddIntAttribute("energy", constants.GetInt("max_energy"));
            player.AddIntAttribute("health", constants.GetInt("max_health"));
            player.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            player.AddIntAttribute("frozen", 0);
            player.AddStringAttribute("collisionPlayer", "");

            Game.Instance.EntityManager.EntityRemoved += new EntityRemovedHandler(entityRemovedHandler);
            jetpackSound = Game.Instance.Content.Load<SoundEffect>("Sounds/jetpack");

            this.player = player;
        }

        public void OnDetached(Entity player)
        {
            player.Update -= OnUpdate;
            Game.Instance.EntityManager.EntityRemoved -= new EntityRemovedHandler(entityRemovedHandler);
        }

        private void OnUpdate(Entity player, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            PlayerIndex playerIndex = (PlayerIndex)player.GetInt("game_pad_index");
            Vector3 playerPosition = player.GetVector3("position");
            Vector3 jetpackVelocity = player.GetVector3("jetpack_velocity");
            Vector3 contactPushbackVelocity = player.GetVector3("contact_pushback_velocity");
            Vector3 hitPushbackVelocity = player.GetVector3("hit_pushback_velocity");
            
            int fuel = player.GetInt("fuel");

            Model playerModel = Game.Instance.Content.Load<Model>(player.GetString("mesh"));

            Vector3 originalPosition = playerPosition;

            // get input
            controllerInput.Update(playerIndex);

            /// movements

            // jetpack
            if (controllerInput.jetpackPressed)
            {
                if (fuel > 0)
                {
                    // indicate 
                    if (gameTime.TotalGameTime.TotalMilliseconds > jetpackSoundEnd)
                    {
                        jetpackSound.Play();
                        jetpackSoundEnd = gameTime.TotalGameTime.TotalMilliseconds + jetpackSound.Duration.TotalMilliseconds;
                    }

                    fuel -= gameTime.ElapsedGameTime.Milliseconds;
                    jetpackVelocity += constants.GetVector3("jetpack_acceleration") * dt;

                    if (jetpackVelocity.Length() > constants.GetFloat("max_jetpack_speed"))
                    {
                        jetpackVelocity.Normalize();
                        jetpackVelocity *= constants.GetFloat("max_jetpack_speed");
                    }
                }
            }
            else
                fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier"));
            if (fuel < 0)
                fuel = 0;
            if (fuel > constants.GetInt("max_fuel"))
                fuel = constants.GetInt("max_fuel");

            // gravity
            if (jetpackVelocity.Length() <= constants.GetFloat("max_gravity_speed"))
                jetpackVelocity += constants.GetVector3("gravity_acceleration") * dt;
            
            playerPosition += jetpackVelocity * dt;

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
                if (activeIsland != null)
                {
                    // TODO: how to do this in future?
                    BoundingCylinder ibc = Game.calculateBoundingCylinder(activeIsland);

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


            // ice spike
            if (controllerInput.firePressed && player.GetInt("energy") > constants.GetInt("ice_spike_energy_cost") &&
                (gameTime.TotalGameTime.TotalMilliseconds - iceSpikeFiredAt) > constants.GetInt("ice_spike_cooldown"))
            {
                // indicate 
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
                soundEffect.Play();

                BoundingBox bb = Game.calculateBoundingBox(player);

                Vector3 pos = new Vector3(playerPosition.X, bb.Max.Y, playerPosition.Z);
                Vector3 velocity = Vector3.Transform(new Vector3(0, 0, 1), Game.GetRotation(player)) * constants.GetFloat("ice_spike_speed");
                velocity.Y = constants.GetFloat("ice_spike_up_speed");

                Entity iceSpike = new Entity(Game.Instance.EntityManager, "icespike" + (++iceSpikeCount)+"_"+player.Name);
                iceSpike.AddStringAttribute("player", player.Name);

                iceSpike.AddVector3Attribute("velocity", velocity);
                iceSpike.AddVector3Attribute("position", pos);

                iceSpike.AddStringAttribute("mesh", "Models/icespike_primitive");
                iceSpike.AddVector3Attribute("scale", new Vector3(5, 5, 5));

                iceSpike.AddProperty("render", new RenderProperty());
                iceSpike.AddProperty("controller", new IceSpikeControllerProperty());

                Game.Instance.EntityManager.AddDeferred(iceSpike);

                // update states
                player.SetInt("energy", player.GetInt("energy") - constants.GetInt("ice_spike_energy_cost"));
                iceSpikeFiredAt = gameTime.TotalGameTime.TotalMilliseconds;
            }

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
            if ((gameTime.TotalGameTime.TotalMilliseconds - energyRechargedAt) > constants.GetInt("energy_recharge_interval"))
            {
                player.SetInt("energy", player.GetInt("energy") + 1);
                energyRechargedAt = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (player.GetInt("energy") > constants.GetInt("max_energy"))
                player.SetInt("energy", constants.GetInt("max_energy"));
            if (player.GetInt("health") > constants.GetInt("max_health"))
                player.SetInt("health", constants.GetInt("max_health"));


            // update player attributes
            if (fuel > constants.GetInt("max_fuel"))
                fuel = constants.GetInt("max_fuel");
            player.SetInt("fuel", fuel);

            player.SetVector3("position", playerPosition);
            player.SetVector3("jetpack_velocity", jetpackVelocity);
            player.SetVector3("contact_pushback_velocity", contactPushbackVelocity);
            player.SetVector3("hit_pushback_velocity", hitPushbackVelocity);







            /// TODO: move this to collision manager
            /// collision detection code

            // get bounding sphere
            BoundingSphere bs = Game.calculateBoundingSphere(player);

            // check collison with islands
            foreach (Entity island in Game.Instance.IslandManager)
            {
                BoundingCylinder ibc = Game.calculateBoundingCylinder(island);
                if (ibc.Intersects(bs))
                    playerIslandCollisionHandler(gameTime, player, island);
                else
                    if (island == activeIsland)
                        playerIslandMissingCollisionHandler(gameTime, player, island);
            }

            // check collison with pillars
            foreach (Entity pillar in Game.Instance.PillarManager)
            {
                BoundingCylinder pbc = Game.calculateBoundingCylinder(pillar);

                if (pbc.Intersects(bs))
                {
                    playerPillarCollisionHandler(gameTime, player, pillar);
                    break;
                }
            }

            // check collision with juicy powerups
            foreach (Entity powerup in Game.Instance.PowerupManager)
            {
                BoundingBox bb = Game.calculateBoundingBox(powerup);

                if (bb.Intersects(bs))
                {
                    playerPowerupCollisionHandler(gameTime, player, powerup);
                }
            }

            // and check collision with other player
            foreach (Entity p in Game.Instance.PlayerManager)
            {
                BoundingSphere obs = Game.calculateBoundingSphere(p);

                if (obs.Intersects(bs) 
                    || player.Name.Equals(p.GetString("collisionPlayer") /* hack before collision manager */))
                {
                    playerPlayerCollisionHandler(gameTime, player, p);
                }
            }

            // check collision with lava
            Entity lava = Game.Instance.EntityManager["lava"];
            if (playerPosition.Y < lava.GetVector3("position").Y)
                playerLavaCollisionHandler(gameTime, player, lava);
        }

        private void playerIslandCollisionHandler(GameTime gameTime, Entity player, Entity island)
        {
            Vector3 playerPosition = player.GetVector3("position");

            BoundingSphere bs = Game.calculateBoundingSphere(player);
            BoundingCylinder ibc = Game.calculateBoundingCylinder(island);

            if (bs.Center.Y - bs.Radius < ibc.Top.Y && bs.Center.Y + bs.Radius > ibc.Top.Y)
            {
                // standing on island

                // remove handler from old active island
                if(activeIsland != null && activeIsland != island)
                    ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= islandPositionHandler;

                // faster recharge standing on island
                player.SetInt("fuel", player.GetInt("fuel") + (int)(gameTime.ElapsedGameTime.Milliseconds * 
                    constants.GetFloat("fuel_recharge_multiplier_island")));

                // correct position to exact touching point
                playerPosition.Y += (bs.Radius - (bs.Center.Y - ibc.Top.Y));
                // add handler if active island changed
                if (activeIsland == null)
                    ((Vector3Attribute)island.Attributes["position"]).ValueChanged += islandPositionHandler;

                activeIsland = island; // mark as active
            }
            else
            {
                // get pseudo center vectors
                Vector3 c = bs.Center;
                Vector3 cc = ibc.Top;
                c.Y = 0; cc.Y = 0;

                if ((c - cc).Length() > ibc.Radius)
                {
                    //                            Console.WriteLine("collision on xz");

                    // pushback in xz
                    Vector3 dir = c - cc;
                    float push = (bs.Radius + ibc.Radius) - dir.Length();

                    dir.Normalize();
                    dir *= push;

                    playerPosition += dir;
                }
                else
                {
                    //                            Console.WriteLine("collision from bottom");

                    // pushback on y
                    float ydist = bs.Radius - (ibc.Bottom.Y - bs.Center.Y);
                    playerPosition.Y -= ydist;
                }
            }

            player.SetVector3("position", playerPosition);
        }

        private void playerIslandMissingCollisionHandler(GameTime gameTime, Entity player, Entity island)
        {
            // remove handler 
            ((Vector3Attribute)island.Attributes["position"]).ValueChanged -= islandPositionHandler;
            activeIsland = null;
        }

        private void playerPillarCollisionHandler(GameTime gameTime, Entity player, Entity pillar)
        {
            Vector3 playerPosition = player.GetVector3("position");

            BoundingSphere bs = Game.calculateBoundingSphere(player);
            BoundingCylinder pbc = Game.calculateBoundingCylinder(pillar);

            // get center vectors
            Vector3 c = bs.Center;
            Vector3 cc = pbc.Top;
            c.Y = 0; cc.Y = 0;

            // and direction
            Vector3 dir = c - cc;
            float push = (bs.Radius + pbc.Radius) - dir.Length();
            dir.Normalize();
            dir *= push;

            playerPosition += dir;

            player.SetVector3("position", playerPosition);
        }

        private void playerLavaCollisionHandler(GameTime gameTime, Entity player, Entity lava)
        {
            player.SetInt("health", 0); // death

            Vector3 playerPosition = player.GetVector3("position");
            playerPosition.Y = lava.GetVector3("position").Y;
            player.SetVector3("position", playerPosition);
        }

        private void playerPowerupCollisionHandler(GameTime gameTime, Entity player, Entity powerup)
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

        private void playerPlayerCollisionHandler(GameTime gameTime, Entity player, Entity otherPlayer)
        {
            if (otherPlayer == player) // dont collide with self!
                return;

            Vector3 playerPosition = player.GetVector3("position");

            BoundingSphere bs = Game.calculateBoundingSphere(player);
            BoundingSphere obs = Game.calculateBoundingSphere(otherPlayer);

            if (obs.Intersects(bs))
                otherPlayer.SetString("collisionPlayer", player.Name); // indicate collision
            else
                otherPlayer.SetString("collisionPlayer", ""); // reset collision

            // and hit?
            if (controllerInput.hitPressed &&
                (gameTime.TotalGameTime.TotalMilliseconds - hitPerformedAt) > constants.GetInt("hit_cooldown"))
            {
                // indicate hit!
                SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
                soundEffect.Play();

                // dedcut health
                otherPlayer.SetInt("health", otherPlayer.GetInt("health") - constants.GetInt("hit_damage"));

                // push back
                Vector3 dir = obs.Center - bs.Center;
                dir.Normalize();
                dir.Y = 0;

                // set values
                otherPlayer.SetVector3("hit_pushback_velocity", dir * constants.GetFloat("hit_pushback_velocity_multiplier"));
                hitPerformedAt = gameTime.TotalGameTime.TotalMilliseconds;
            }
            else
            {
                // normal feedback
                Vector3 push = bs.Center - obs.Center;
                push *= (obs.Radius + bs.Radius) - push.Length();
                push.Normalize();

                player.SetVector3("contact_pushback_velocity", push * constants.GetFloat("pushback_contact_velocity_multiplier") / 2);
                otherPlayer.SetVector3("contact_pushback_velocity", push * -constants.GetFloat("pushback_contact_velocity_multiplier") / 2);
            }
        }

        private void islandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = player.GetVector3("position");
            Vector3 delta = newValue - oldValue;
            position += delta;
            player.SetVector3("position", position);
        }

        private void entityRemovedHandler(EntityManager manager, Entity entity)
        {
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

        private Entity player;
        private Entity constants;
        private Entity activeIsland = null;

        private float energyRechargedAt = 0;

        private int iceSpikeCount = 0;
        private int iceSpikeRemovedCount = 0;
        private double iceSpikeFiredAt = 0;

        private double hitPerformedAt = 0;

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
                    else
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
                }

                #endregion

                #region action buttons

                jetpackPressed =
                    gamePadState.Buttons.A == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.Insert) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Space) && playerIndex == PlayerIndex.Two);

                firePressed =
                    gamePadState.Buttons.X == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.RightControl) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.Q) && playerIndex == PlayerIndex.Two);

                hitPressed =
                    gamePadState.Buttons.RightShoulder == ButtonState.Pressed ||
                    (keyboardState.IsKeyDown(Keys.Enter) && playerIndex == PlayerIndex.One) ||
                    (keyboardState.IsKeyDown(Keys.E) && playerIndex == PlayerIndex.Two);

                #endregion

            }

            // in order to use the following variables as private with getters/setters, do
            // we really need 15 lines per variable?!
            
            // joysticks
            public float leftStickX, leftStickY;
            public bool moveStickPressed;

            // buttons
            public bool jetpackPressed, firePressed, hitPressed;

            private static float gamepadEmulationValue = -1f;
        }

        ControllerInput controllerInput;
    }
}
