using System;
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
            Model playerModel = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));

            // get input
            controllerInput.Update(playerIndex);

            // detect collision with islands
            Entity playerIsland = null;
            Boolean playerLava = false;

            // get bounding sphere
            BoundingSphere bs = calculateBoundingSphere(playerModel, playerPosition, entity.GetVector3("scale"));

            // get all islands
            foreach (Entity island in Game.Instance.IslandManager)
            {
                BoundingSphere ibs = calculateBoundingSphere(Game.Instance.Content.Load<Model>(island.GetString("mesh")),
                    island.GetVector3("position"), island.GetVector3("scale"));
                BoundingBox ibb = (BoundingBox)Game.Instance.Content.Load<Model>(island.GetString("mesh")).Tag;
                ibb = new BoundingBox(ibb.Min * island.GetVector3("scale") + island.GetVector3("position"), 
                    ibb.Max * island.GetVector3("scale") + island.GetVector3("position"));

                if (ibs.Intersects(bs) && ibb.Intersects(bs))
                {
                    playerIsland = island;
                    jetpackVelocity = Vector3.Zero;
                    break;
                }
            }

            // check collision with lava
            Entity lava = Game.Instance.EntityManager["lava"];
            BoundingBox lbox = (BoundingBox)(Game.Instance.Content.Load<Model>(lava.GetString("mesh")).Tag);
            Vector3 lpos = lava.GetVector3("position");
            Vector3 lscale = lava.GetVector3("scale");
            lbox = new BoundingBox(lpos + lbox.Min * lscale, lpos + lbox.Max * lscale);

            if (lbox.Intersects(bs))
            {
                jetpackVelocity = Vector3.Zero;
                playerLava = true;
            }

            // jetpack
            if (controllerInput.aPressed)
            {
                jetpackVelocity += jetpackAcceleration * dt;

                if (jetpackVelocity.Length() > maxJetpackSpeed)
                {
                    jetpackVelocity.Normalize();
                    jetpackVelocity *= maxJetpackSpeed;
                }
            }

            // ice spike
            if (controllerInput.xPressed)
            {
                // HACK!
                EntityManager temporaryIceSpikeManager; // to satisfy the entity constructor
                int iceSpikeCount = 0; // this should later be in the corresponding manager
                Entity iceSpike = new Entity(null, "icespike" + (++iceSpikeCount));
            }

            // gravity
            if (playerIsland == null && !playerLava && jetpackVelocity.Length() <= maxGravitySpeed)
                jetpackVelocity += gravityAcceleration * dt;
            else
                if (playerIsland != null)
                    playerPosition += playerIsland.GetVector3("velocity") * dt;

            playerPosition += jetpackVelocity * dt;

            // XZ movement
            playerPosition.X += controllerInput.leftStickX;
            playerPosition.Z -= controllerInput.leftStickY;
            if (controllerInput.leftStickPressed)
            {
                entity.SetFloat("y_rotation", (float)Math.Atan2(controllerInput.leftStickX, -controllerInput.leftStickY));
            }

            // update entity attributes
            entity.SetVector3("position", playerPosition);
            entity.SetVector3("jetpackVelocity", jetpackVelocity);
        }

        private BoundingSphere calculateBoundingSphere(Model model, Vector3 position, Vector3 scale)
        {
            // calculate center
            Vector3 center = scale;
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere sp = mesh.BoundingSphere;
                center *= sp.Center;
            }
            center /= model.Meshes.Count;

            // calculate radius
            float radius = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                Vector3 dist = mesh.BoundingSphere.Center * scale;
                float r = (dist - center).Length() + mesh.BoundingSphere.Radius * scale.Length();
                if (r > radius)
                    radius = r;
            }

            return new BoundingSphere(center + position, radius);
        }

        private float maxJetpackSpeed = 150f;
        private float maxGravitySpeed = 450f;
        private Vector3 gravityAcceleration = new Vector3(0, -900f, 0);

        private int playerXAxisMultiplier = 1;
        private int playerZAxisMultiplier = 2;

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
