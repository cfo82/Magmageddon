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

        public void OnAttached(Entity entity)
        {
            entity.Update += OnUpdate;

            this.constants = Game.Instance.EntityManager["player_constants"];

            entity.AddQuaternionAttribute("rotation", Quaternion.Identity);
            entity.AddVector3Attribute("jetpack_velocity", Vector3.Zero);

            entity.AddVector3Attribute("contact_pushback_velocity", Vector3.Zero);
            entity.AddVector3Attribute("hit_pushback_velocity", Vector3.Zero);

            entity.AddIntAttribute("energy", constants.GetInt("max_energy"));
            entity.AddIntAttribute("health", constants.GetInt("max_health"));
            entity.AddIntAttribute("fuel", constants.GetInt("max_fuel"));

            entity.AddIntAttribute("frozen", 0);
            entity.AddStringAttribute("collisionPlayer", "");

            Game.Instance.EntityManager.EntityRemoved += new EntityRemovedHandler(entityRemovedHandler);
            jetpackSound = Game.Instance.Content.Load<SoundEffect>("Sounds/jetpack");

            this.player = entity;
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            Game.Instance.EntityManager.EntityRemoved -= new EntityRemovedHandler(entityRemovedHandler);
        }

        private Vector3 GetPosition(Entity entity)
        {
            return entity.GetVector3("position");
        }

        private Vector3 GetScale(Entity entity)
        {
            if (entity.HasVector3("scale"))
            {
                return entity.GetVector3("scale");
            }
            else
            {
                return Vector3.One;
            }
        }

        private Quaternion GetRotation(Entity entity)
        {
            if (entity.HasQuaternion("rotation"))
            {
                return entity.GetQuaternion("rotation");
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        private void OnUpdate(Entity entity, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            PlayerIndex playerIndex = (PlayerIndex)entity.GetInt("game_pad_index");
            Vector3 playerPosition = entity.GetVector3("position");
            Vector3 jetpackVelocity = entity.GetVector3("jetpack_velocity");
            Vector3 contactPushbackVelocity = entity.GetVector3("contact_pushback_velocity");
            Vector3 hitPushbackVelocity = entity.GetVector3("hit_pushback_velocity");
            
            int fuel = entity.GetInt("fuel");

            Model playerModel = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));

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
                playerPosition.X += controllerInput.leftStickX * constants.GetFloat("x_axis_jetpack_multiplier");
                playerPosition.Z -= controllerInput.leftStickY * constants.GetFloat("z_axis_jetpack_multiplier");
            }
            else
            {
                /// TODO
                /// support the player better by helping him navigate on border of island:
                /// dont just prevent movement, but allow in one axis and adapt other axis accordingly
                /// so he stays on island

                // on ground
                playerPosition.X += controllerInput.leftStickX * constants.GetFloat("x_axis_movement_multiplier");
                playerPosition.Z -= controllerInput.leftStickY * constants.GetFloat("z_axis_movement_multiplier");

                // prevent the player from walking down the island
                if (activeIsland != null)
                {
                    BoundingCylinder ibc = Game.calculateBoundingCylinder(Game.Instance.Content.Load<Model>(
                         activeIsland.GetString("mesh")), GetPosition(activeIsland), GetRotation(activeIsland), GetScale(activeIsland));
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
                entity.SetQuaternion("rotation", Quaternion.CreateFromRotationMatrix(rotationMatrix));
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

                BoundingBox bb = Game.calculateBoundingBox(playerModel, playerPosition, GetRotation(player), GetScale(player));

                Vector3 pos = new Vector3(playerPosition.X, bb.Max.Y, playerPosition.Z);
                Vector3 velocity = Vector3.Transform(new Vector3(0, 0, 1), GetRotation(player)) * constants.GetFloat("ice_spike_speed");
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


            /// collision detection code

            // get bounding sphere
            BoundingSphere bs = Game.calculateBoundingSphere(playerModel, playerPosition, GetRotation(entity), GetScale(entity));

            // check collison with islands
            Entity newActiveIsland = null;
            foreach (Entity island in Game.Instance.IslandManager)
            {
                BoundingCylinder ibc = Game.calculateBoundingCylinder(Game.Instance.Content.Load<Model>(
                     island.GetString("mesh")), GetPosition(island), GetRotation(island), GetScale(island));

                if (ibc.Intersects(bs))
                {
                    if (bs.Center.Y - bs.Radius < ibc.Top.Y
                        && bs.Center.Y + bs.Radius > ibc.Top.Y)
                    {
//                        Console.WriteLine("collision from top");

                        // correct position to exact touching point
                        playerPosition.Y += (bs.Radius - (bs.Center.Y - ibc.Top.Y));
                        if (activeIsland == null) // add handler
                            ((Vector3Attribute)island.Attributes["position"]).ValueChanged += islandPositionHandler;
                        newActiveIsland = island; // mark as active
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
                    break;
                }
            }
            if (newActiveIsland == null && activeIsland != null)
                ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= islandPositionHandler;
            activeIsland = newActiveIsland;
            if(activeIsland != null) // faster recharge standing on island
                fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * constants.GetFloat("fuel_recharge_multiplier_island"));

            // check collison with pillars
            foreach (Entity pillar in Game.Instance.PillarManager)
            {
                /*
                BoundingCylinder ibc = new BoundingCylinder(island.GetVector3("position") * island.GetVector3("scale") + new Vector3(0, 10, 0),
                    island.GetVector3("position") * island.GetVector3("scale") + new Vector3(0, -10, 0), 30);
                 */
                BoundingCylinder pbc = Game.calculateBoundingCylinder(Game.Instance.Content.Load<Model>(pillar.GetString("mesh")),
                    GetPosition(pillar), GetRotation(pillar), GetScale(pillar));

                if (pbc.Intersects(bs))
                {
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
                    break;
                }
            }
            // check collision with lava
            Entity lava = Game.Instance.EntityManager["lava"];
            if(playerPosition.Y < lava.GetVector3("position").Y)
            {
                playerPosition.Y = originalPosition.Y;
                player.SetInt("health", 0);
            }

            // check collision with juicy powerups
            foreach (Entity powerup in Game.Instance.PowerupManager)
            {
                BoundingBox bb = Game.calculateBoundingBox(Game.Instance.Content.Load<Model>(
                    powerup.GetString("mesh")), 
                    GetPosition(powerup), GetRotation(powerup), GetScale(powerup));

                if (bb.Intersects(bs))
                {
                    Game.Instance.EntityManager.RemoveDeferred(powerup);

                    // use the power
                    int oldVal = player.GetInt(powerup.GetString("power"));
                    oldVal += powerup.GetInt("powerValue");
                    player.SetInt(powerup.GetString("power"), oldVal);

                    // soundeffect
                    SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/"+powerup.GetString("pickup_sound"));
                    soundEffect.Play();
                }
            }

            // and check collision with other player
            foreach (Entity p in Game.Instance.PlayerManager)
            {
                if (p == player) // dont collide with self!
                    continue; 

                BoundingSphere obs = Game.calculateBoundingSphere(Game.Instance.Content.Load<Model>(p.GetString("mesh")),
                    GetPosition(p), GetRotation(p), GetScale(p));

                if (obs.Intersects(bs) || player.Name.Equals(p.GetString("collisionPlayer")))
                {
                    if(obs.Intersects(bs))
                        p.SetString("collisionPlayer", player.Name); // indicate collision
                    else
                        p.SetString("collisionPlayer", ""); // reset collision

                    // and hit?
                    if (controllerInput.hitPressed &&
                        (gameTime.TotalGameTime.TotalMilliseconds - hitPerformedAt) > constants.GetInt("hit_cooldown"))
                    {
                        // indicate hit!
                        SoundEffect soundEffect = Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
                        soundEffect.Play();

                        // dedcut health
                        p.SetInt("health", p.GetInt("health") - constants.GetInt("hit_damage"));

                        // push back
                        Vector3 dir = obs.Center - bs.Center;
                        dir.Normalize();
                        dir.Y = 0;

                        // set values
                        p.SetVector3("hit_pushback_velocity", dir * constants.GetFloat("hit_pushback_velocity_multiplier"));
                        hitPerformedAt = gameTime.TotalGameTime.TotalMilliseconds;
                    }
                    else
                    {
                        // normal feedback
                        Vector3 push = bs.Center - obs.Center;
                        push *= (obs.Radius + bs.Radius) - push.Length();
                        push.Normalize();

                        player.SetVector3("contact_pushback_velocity", push * constants.GetFloat("pushback_contact_velocity_multiplier") / 2);
                        p.SetVector3("contact_pushback_velocity", push * -constants.GetFloat("pushback_contact_velocity_multiplier") / 2);
                    }
                }
            }


            // recharge energy
            if ((gameTime.TotalGameTime.TotalMilliseconds - energyRechargedAt) > constants.GetInt("energy_recharge_interval"))
            {
                entity.SetInt("energy", entity.GetInt("energy") + 1);
                energyRechargedAt = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (player.GetInt("energy") > constants.GetInt("max_energy"))
                player.SetInt("energy", constants.GetInt("max_energy"));
            if (player.GetInt("health") > constants.GetInt("max_health"))
                player.SetInt("health", constants.GetInt("max_health"));


            // update entity attributes
            if (fuel > constants.GetInt("max_fuel"))
                fuel = constants.GetInt("max_fuel");
            entity.SetInt("fuel", fuel);

            entity.SetVector3("position", playerPosition);
            entity.SetVector3("jetpack_velocity", jetpackVelocity);
            entity.SetVector3("contact_pushback_velocity", contactPushbackVelocity);
            entity.SetVector3("hit_pushback_velocity", hitPushbackVelocity);
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
