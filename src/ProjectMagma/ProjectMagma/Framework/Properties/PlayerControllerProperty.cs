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
        public PlayerControllerProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            entity.Update += OnUpdate;

            entity.AddQuaternionAttribute("rotation", Quaternion.Identity);
            entity.AddVector3Attribute("jetpack_velocity", Vector3.Zero);

            entity.AddVector3Attribute("pushback_velocity", Vector3.Zero);
            entity.AddVector3Attribute("pushback_deacceleration", Vector3.Zero);

            entity.AddIntAttribute("energy", maxEnergy);
            entity.AddIntAttribute("health", maxHealth);
            entity.AddIntAttribute("fuel", maxFuel);

            entity.AddIntAttribute("frozen", 0);

            this.player = entity;
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
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
            Vector3 jetpackAcceleration = entity.GetVector3("jetpack_acceleration");
            Vector3 playerPosition = entity.GetVector3("position");
            Vector3 jetpackVelocity = entity.GetVector3("jetpack_velocity");
            Vector3 pushbackVelocity = entity.GetVector3("pushback_velocity");
            
            int fuel = entity.GetInt("fuel");

            Model playerModel = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));

            Vector3 originalPosition = playerPosition;

            // get input
            controllerInput.Update(playerIndex);

            /// TODO: jetpack is a bit jearky right now, as before velocity gets a certain amount
            /// player always collides with island and the positon correction code from island/player collisin
            /// gets executed


            /// movements

            // jetpack
            if (controllerInput.aPressed)
            {
                if (fuel > 0)
                {
                    fuel -= gameTime.ElapsedGameTime.Milliseconds;
                    jetpackVelocity += jetpackAcceleration * dt;

                    if (jetpackVelocity.Length() > maxJetpackSpeed)
                    {
                        jetpackVelocity.Normalize();
                        jetpackVelocity *= maxJetpackSpeed;
                    }
                }
            }
            else
                fuel += (int)(gameTime.ElapsedGameTime.Milliseconds * fuelRechargeMultiplicator);
            if (fuel < 0)
                fuel = 0;
            if (fuel > maxFuel)
                fuel = maxFuel;

            // gravity
            if (jetpackVelocity.Length() <= maxGravitySpeed)
                jetpackVelocity += gravityAcceleration * dt;
            
            playerPosition += jetpackVelocity * dt;

            // pushback
            if (pushbackVelocity.Length() > 0)
            {
                Vector3 oldVelocity = pushbackVelocity;
                Vector3 pushbackDeAcceleration = pushbackVelocity;
                pushbackDeAcceleration.Normalize();

                pushbackVelocity -= pushbackDeAcceleration * pushbackDeAccelerationMultiplier * dt;
                if (pushbackVelocity.Length() > oldVelocity.Length()) // if length increases we accelerate -> stop
                    pushbackVelocity = Vector3.Zero;

                playerPosition += pushbackVelocity * dt;
            }

            // XZ movement
            playerPosition.X += controllerInput.leftStickX * playerXAxisMultiplier;
            playerPosition.Z -= controllerInput.leftStickY * playerZAxisMultiplier;

            // rotation
            if (controllerInput.leftStickPressed)
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
            if (controllerInput.xPressed && player.GetInt("energy") > iceSpikeEnergyCost &&
                (gameTime.TotalGameTime.TotalMilliseconds - iceSpikeFiredAt) > iceSpikeCooldown)
            {
                BoundingBox bb = Game.calculateBoundingBox(playerModel, playerPosition, GetRotation(player), GetScale(player));

                Vector3 pos = new Vector3(playerPosition.X, bb.Max.Y, playerPosition.Z);
                Vector3 velocity = Vector3.Transform(Vector3.One * iceSpikeSpeed, GetRotation(player));
                velocity.Y = iceSpikeUpSpeed;

                Entity iceSpike = new Entity(Game.Instance.EntityManager, "icespike" + (++iceSpikeCount));
                iceSpike.AddStringAttribute("player", player.Name);

                iceSpike.AddVector3Attribute("velocity", velocity);
                iceSpike.AddVector3Attribute("position", pos);

                iceSpike.AddStringAttribute("mesh", "Models/icespike_primitive");
                iceSpike.AddVector3Attribute("scale", new Vector3(5, 5, 5));

                iceSpike.AddProperty("render", new RenderProperty());
                iceSpike.AddProperty("controller", new IceSpikeControllerProperty());

                Game.Instance.EntityManager.AddDeferred(iceSpike);

                // update states
                /// 
                /// TODO: uncomment to get correct energy deduction

                player.SetInt("energy", player.GetInt("energy") - iceSpikeEnergyCost);
                iceSpikeFiredAt = gameTime.TotalGameTime.TotalMilliseconds;
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
            if(newActiveIsland == null && activeIsland != null)
                ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= islandPositionHandler;
            activeIsland = newActiveIsland;

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
            }

            // check collision with juicy powerups
            foreach (Entity powerup in Game.Instance.EntityManager)
            {
                if (powerup.Name.StartsWith("powerup"))
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
            }

            // and check collision with other player
            foreach (Entity p in Game.Instance.EntityManager)
            {
                if (p.Name.StartsWith("player") && p != player)
                {
                    BoundingSphere obs = Game.calculateBoundingSphere(Game.Instance.Content.Load<Model>(p.GetString("mesh")),
                        GetPosition(p), GetRotation(p), GetScale(p));

                    if (obs.Intersects(bs))
                    {
                        // and hit?
                        if (controllerInput.rPressed)
                        {
                            // dedcut health
                            p.SetInt("health", p.GetInt("health") - 20);

                            // push back
                            Vector3 dir = obs.Center - bs.Center;
                            dir.Normalize();
                            dir.Y = 0;

                            p.SetVector3("pushback_velocity", dir * pushbackHitVelocityMultiplier);
                        }
                        else
                        {
                            // normal feedback
                            Vector3 push = bs.Center - obs.Center;
                            push *= (obs.Radius + bs.Radius) - push.Length();
                            push.Normalize();

                            player.SetVector3("pushback_velocity", push * pushbackPlay2PlayerVelocityMultiplier);
                            p.SetVector3("pushback_velocity", push * -pushbackPlay2PlayerVelocityMultiplier);
                        }
                    }
                }
            }


            // recharge energy
            if ((gameTime.TotalGameTime.TotalMilliseconds - energyRechargedAt) > energyRechargIntervall)
            {
                entity.SetInt("energy", entity.GetInt("energy") + 1);
                energyRechargedAt = (float)gameTime.TotalGameTime.TotalMilliseconds;
            }

            if (player.GetInt("energy") > maxEnergy)
                player.SetInt("energy", maxEnergy);
            if (player.GetInt("health") > maxHealth)
                player.SetInt("health", maxHealth);


            // update entity attributes

            /// TODO: allow update again
//            entity.SetInt("fuel", fuel);

            entity.SetVector3("position", playerPosition);
            entity.SetVector3("jetpack_velocity", jetpackVelocity);
            entity.SetVector3("pushback_velocity", pushbackVelocity);
        }

        private void islandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = player.GetVector3("position");
            Vector3 delta = newValue - oldValue;
            position += delta;
            player.SetVector3("position", position);
        }

        private Entity player;
        private Entity activeIsland = null;

        private static readonly int maxHealth = 100;
        private static readonly int maxEnergy = 100;
        private static readonly int maxFuel = 1500;

        private static float energyRechargedAt = 0;
        private static readonly int energyRechargIntervall = 250; // ms

        private static readonly float fuelRechargeMultiplicator = 0.75f;
        private static readonly float maxJetpackSpeed = 150f;
        private static readonly float maxGravitySpeed = 450f;
        private static readonly Vector3 gravityAcceleration = new Vector3(0, -900f, 0);

        private static readonly float pushbackPlay2PlayerVelocityMultiplier = 80f;
        private static readonly float pushbackHitVelocityMultiplier = 180f;
        private static readonly float pushbackDeAccelerationMultiplier = 300f;

        private int iceSpikeCount = 0;
        private double iceSpikeFiredAt = 0;
        private static readonly int iceSpikeSpeed = 130;
        private static readonly int iceSpikeUpSpeed = 240;
        private static readonly int iceSpikeEnergyCost = 4;
        private static readonly int iceSpikeCooldown = 50; // ms

        private static readonly int playerXAxisMultiplier = 1;
        private static readonly int playerZAxisMultiplier = 2;

        struct ControllerInput
        {
            public void Update(PlayerIndex playerIndex)
            {
                GamePadState gamePadState = GamePad.GetState(playerIndex);
                KeyboardState keyboardState = Keyboard.GetState(playerIndex);

                #region joysticks

                leftStickX = gamePadState.ThumbSticks.Left.X;
                leftStickY = gamePadState.ThumbSticks.Left.Y;
                leftStickPressed = leftStickX != 0.0f || leftStickY != 0.0f;

                if(!leftStickPressed)
                {
                    if(keyboardState.IsKeyDown(Keys.Left))
                    {
                        leftStickX = gamepadEmulationValue;
                        leftStickPressed = true;
                    } 
                    else
                        if(keyboardState.IsKeyDown(Keys.Right))
                        {
                            leftStickX = -gamepadEmulationValue;
                            leftStickPressed = true;
                        }
                    
                    if(keyboardState.IsKeyDown(Keys.Up))
                    {
                        leftStickY = -gamepadEmulationValue;
                        leftStickPressed = true;
                    } 
                    else
                        if(keyboardState.IsKeyDown(Keys.Down))
                        {
                            leftStickY = gamepadEmulationValue;
                            leftStickPressed = true;
                        }
                }

                #endregion

                #region action buttons

                aPressed =
                    gamePadState.Buttons.A == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.Space);

                xPressed =
                    gamePadState.Buttons.X == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.Enter);

                bPressed =
                    gamePadState.Buttons.B == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.Back);

                yPressed =
                    gamePadState.Buttons.Y == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.RightShift);

                lPressed =
                    gamePadState.Buttons.LeftShoulder == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.LeftAlt);

                rPressed =
                    gamePadState.Buttons.RightShoulder == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.LeftControl);

                #endregion

            }

            // in order to use the following variables as private with getters/setters, do
            // we really need 15 lines per variable?!
            
            // joysticks
            public float leftStickX, leftStickY;
            public bool leftStickPressed;

            // buttons
            public bool aPressed, xPressed;
            public bool bPressed, yPressed;
            public bool lPressed, rPressed;

            private static float gamepadEmulationValue = -1f;
        }

        ControllerInput controllerInput;
    }
}
