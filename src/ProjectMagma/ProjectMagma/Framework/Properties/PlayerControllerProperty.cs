﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectMagma.Framework
{
    public class PlayerControllerProperty : Property
    {
        public PlayerControllerProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            entity.Update += new UpdateHandler(OnUpdate);

            entity.AddAttribute("y_rotation", "float", "0");
            entity.AddAttribute("jetpackVelocity", "float3", "0 0 0");
            entity.AddAttribute("energy", "int", "100");
            entity.AddAttribute("health", "int", "100");
            entity.AddAttribute("fuel", "int", ""+maxFuel);

            this.player = entity;
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= new UpdateHandler(OnUpdate);
            // TODO: remove attribute!
        }

        private void OnUpdate(Entity entity, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            PlayerIndex playerIndex = (PlayerIndex)entity.GetInt("gamePadIndex");
            Vector3 jetpackAcceleration = entity.GetVector3("jetpackAcceleration");
            Vector3 playerPosition = entity.GetVector3("position");
            Vector3 jetpackVelocity = entity.GetVector3("jetpackVelocity");
            int fuel = entity.GetInt("fuel");

            Model playerModel = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));

            Vector3 originalPosition = playerPosition;

            // get input
            controllerInput.Update(playerIndex);

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

            // ice spike
            if (controllerInput.xPressed)
            {
                int iceSpikeCount = 0; // this should later be in the corresponding manager
                Entity iceSpike = new Entity(Game.Instance.EntityManager, "icespike" + (++iceSpikeCount));
                Game.Instance.EntityManager.AddDeferred(iceSpike);
            }

            // gravity
            if (jetpackVelocity.Length() <= maxGravitySpeed)
                jetpackVelocity += gravityAcceleration * dt;
            
            playerPosition += jetpackVelocity * dt;

            // XZ movement
            playerPosition.X += controllerInput.leftStickX * playerXAxisMultiplier;
            playerPosition.Z -= controllerInput.leftStickY * playerZAxisMultiplier;

            if (controllerInput.leftStickPressed)
                entity.SetFloat("y_rotation", (float)Math.Atan2(controllerInput.leftStickX, -controllerInput.leftStickY));

            // get bounding sphere
            BoundingSphere bs = calculateBoundingSphere(playerModel, playerPosition, entity.GetVector3("scale"));

            // check collison with islands
            Entity newActiveIsland = null;
            foreach (Entity island in Game.Instance.IslandManager)
            {
                 BoundingCylinder ibc = calculateBoundingCylinder(Game.Instance.Content.Load<Model>(island.GetString("mesh")),
                    island.GetVector3("position"), island.GetVector3("scale"));

                if (ibc.Intersects(bs))
                {
                    if (bs.Center.Y + bs.Radius > ibc.Top.Y)
                    {
                        playerPosition.Y = ibc.Top.Y + bs.Radius;
                        if(activeIsland == null)
                            ((Vector3Attribute)island.Attributes["position"]).ValueChanged += new Vector3ChangeHandler(islandPositionHandler);
                        newActiveIsland = island;
                    }
                    else
                        playerPosition = originalPosition;
                    break;
                }
            }
            if(newActiveIsland == null && activeIsland != null)
                ((Vector3Attribute)activeIsland.Attributes["position"]).ValueChanged -= new Vector3ChangeHandler(islandPositionHandler);
            activeIsland = newActiveIsland;

            // check collison with pillars
            foreach (Entity pillar in Game.Instance.PillarManager)
            {
                /*
                BoundingCylinder ibc = new BoundingCylinder(island.GetVector3("position") * island.GetVector3("scale") + new Vector3(0, 10, 0),
                    island.GetVector3("position") * island.GetVector3("scale") + new Vector3(0, -10, 0), 30);
                 */
                BoundingCylinder ibc = calculateBoundingCylinder(Game.Instance.Content.Load<Model>(pillar.GetString("mesh")),
                    pillar.GetVector3("position"), pillar.GetVector3("scale"));

                if (ibc.Intersects(bs))
                {
                    playerPosition = originalPosition;
                    break;
                }
            }
            // check collision with lava
            Entity lava = Game.Instance.EntityManager["lava"];
            if(playerPosition.Y < lava.GetVector3("position").Y)
            {
                playerPosition.Y = originalPosition.Y;
            }


            // update entity attributes
            entity.SetInt("fuel", fuel);
            entity.SetVector3("position", playerPosition);
            entity.SetVector3("jetpackVelocity", jetpackVelocity);
        }

        private void islandPositionHandler(Vector3Attribute sender, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 position = player.GetVector3("position");
            position += newValue - oldValue;
            player.SetVector3("position", position);
        }

        private BoundingSphere calculateBoundingSphere(Model model, Vector3 position, Vector3 scale)
        {
            // calculate center
            BoundingBox bb = calculateBoundingBox(model, position, scale);
            Vector3 center = (bb.Min + bb.Max) / 2;

            // calculate radius
//            float radius = (bb.Max-bb.Min).Length() / 2;
            float radius = (bb.Max.Y - bb.Min.Y) / 2; // hack for player

            return new BoundingSphere(center, radius);
        }

        // calculates y-axis aligned bounding cylinder
        private BoundingCylinder calculateBoundingCylinder(Model model, Vector3 position, Vector3 scale)
        {
            // calculate center
            BoundingBox bb = calculateBoundingBox(model, position, scale);
            Vector3 center = (bb.Min + bb.Max) / 2;

            float top = bb.Max.Y;
            float bottom = bb.Min.Y;

            // calculate radius
            // a valid cylinder here is an extruded circle (not an oval) therefore extents in 
            // x- and z-direction should be equal.
            float radius = bb.Max.X - center.X;

            return new BoundingCylinder(new Vector3(center.X, top, center.Z),
                new Vector3(center.X, bottom, center.Z),
                radius);
        }

        private BoundingBox calculateBoundingBox(Model model, Vector3 position, Vector3 scale)
        {
            BoundingBox bb = calculateBoundingBox(model, scale);
            return new BoundingBox(bb.Min + position, bb.Max + position);
        }

        private BoundingBox calculateBoundingBox(Model model, Vector3 scale)
        {
            BoundingBox bb = (BoundingBox)model.Tag;
            bb.Min = new Vector3(bb.Min.X * scale.X, bb.Min.Y * scale.Y, bb.Min.Z * scale.Z);
            bb.Max = new Vector3(bb.Max.X * scale.X, bb.Max.Y * scale.Y, bb.Max.Z * scale.Z);
            return bb;
            /*
            Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int stride = part.VertexStride;
                    int numberv = part.NumVertices;
                    VertexDeclaration test1 = part.VertexDeclaration;      // not used for now
                    byte[] data = new byte[stride * numberv];

                    mesh.VertexBuffer.GetData<byte>(data);

                    for (int ndx = 0; ndx < data.Length; ndx += stride)
                    {
                        float floatvaluex = BitConverter.ToSingle(data, ndx) * scale.X;
                        float floatvaluey = BitConverter.ToSingle(data, ndx + 4) * scale.Y;
                        float floatvaluez = BitConverter.ToSingle(data, ndx + 8) * scale.Z;
                        if (floatvaluex < Min.X) Min.X = floatvaluex;
                        if (floatvaluex > Max.X) Max.X = floatvaluex;
                        if (floatvaluey < Min.Y) Min.Y = floatvaluey;
                        if (floatvaluey > Max.Y) Max.Y = floatvaluey;
                        if (floatvaluez < Min.Z) Min.Z = floatvaluez;
                        if (floatvaluez > Max.Z) Max.Z = floatvaluez;
                    }
                }
            }

            BoundingBox boundingbox = new BoundingBox(Min, Max);  // presto  
            return boundingbox;*/
        }  
   

        private struct BoundingCylinder
        {
            private Vector3 c1;
            private Vector3 c2;
            private float radius;

            public BoundingCylinder(Vector3 c1, Vector3 c2, float radius)
            {
                this.c1 = c1;
                this.c2 = c2;
                this.radius = radius;
            }

            public bool Intersects(BoundingSphere bs)
            {
                // check collision on y axis
                if (bs.Center.Y - bs.Radius < c1.Y && bs.Center.Y + bs.Radius > c2.Y)
                {
                    // check collision in xz
                    if(pow2(bs.Center.X-c1.X) + pow2(bs.Center.Z-c1.Z) < pow2(bs.Radius + radius))
                        return true; 
                }

                return false;
            }

            public Vector3 Top
            {
                get { return c1; }
            }

            public Vector3 Bottom
            {
                get { return c2; }
            }

            public float Radius
            {
                get { return radius; }
            }

        }

        private static float pow2(float a)
        {
            return a * a;
        }

        private Entity player;
        private Entity activeIsland = null;

        private static readonly int maxFuel = 20000; //1500;
        private static readonly float fuelRechargeMultiplicator = 0.8f;
        private static readonly float maxJetpackSpeed = 150f;
        private static readonly float maxGravitySpeed = 450f;
        private static readonly Vector3 gravityAcceleration = new Vector3(0, -900f, 0);

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

                #endregion

            }

            // in order to use the following variables as private with getters/setters, do
            // we really need 15 lines per variable?!
            
            // joysticks
            public float leftStickX, leftStickY;
            public bool leftStickPressed;
            public float rightStickX, rightStickY;
            public bool rightStickPressed;

            // buttons
            public bool aPressed, bPressed, xPressed, yPressed;
            public bool ltPressed, rtPressed;
            public bool lbPressed, rbPressed;
            public bool startPressed;

            private static float gamepadEmulationValue = -1f;
        }

        ControllerInput controllerInput;
    }
}
