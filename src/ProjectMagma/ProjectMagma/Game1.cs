using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Serialization.LevelData;


// its worth to read this...
// 
// http://msdn.microsoft.com/en-us/library/microsoft.xna.net_cf.system.threading.thread.setprocessoraffinity.aspx

namespace ProjectMagma
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model islandPrimitive;
        Model lavaPrimitive;
        Model pillarPrimitive;
        Model playerPrimitive;
        Matrix world;
        Matrix view;
        Matrix projection;

        LevelData levelData;

        Simulation simulation;

        Vector3 playerPosition;
        Vector3 jetpackAcceleration;
        Vector3 jetpackSpeed = new Vector3(0,0,0);
        Vector3 gravityAcceleration = new Vector3(0, -120f, 0);
        float maxJetpackSpeed = 100f;
        float maxGravitySpeed = 100f;
        Entity playerIsland = null;

        int playerXAxisMultiplier = 1;
        int playerZAxisMultiplier = 2;


        float islandDamping = 0.001f;
        float islandRandomStrength = 1000.0f;
        float islandMaxVelocity = 200;
        float pillarElasticity = 0.1f;
        float pillarAttraction = 0.0005f;
        float pillarRepulsion = 0.03f;
        float pillarIslandCollisionRadius = 50.0f;


        Random rand;

        List<Entity> pillars;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            simulation = new Simulation();
            Content.RootDirectory = "Content";

            rand = new Random(485394);
            pillars = new List<Entity>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is theF place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            islandPrimitive = Content.Load<Model>("Models/island_primitive");
            lavaPrimitive = Content.Load<Model>("Models/lava_primitive");
            pillarPrimitive = Content.Load<Model>("Models/pillar_primitive");
            playerPrimitive = Content.Load<Model>("Models/player_primitive");
            
            levelData = Content.Load<LevelData>("Level/TestLevel");

            simulation.Initialize(Content, levelData);

            simulation.AttributeTemplateManager.AddAttributeTemplate("General.CollisionCount", typeof(IntAttribute).FullName);
            foreach (Entity e in simulation.EntityManager.Entities.Values)
            {
                if (e.Name.StartsWith("island"))
                {
                    e.AddAttribute(Content, "collisionCount", "General.CollisionCount", "0");
                }
            }

            foreach (Entity e in simulation.EntityManager.Entities.Values)
            {
                if (e.Name.StartsWith("pillar"))
                {
                    pillars.Add(e);
                }
            }         

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            view = Matrix.CreateLookAt(new Vector3(0, 500, 1000),
                                       new Vector3(0, 150, 0), Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                             aspectRatio, 10, 10000);
            // TODO: use this.Content to load your game content here

            playerPosition = ((Vector3Attribute)simulation.EntityManager.Entities["player"].Attributes["position"]).Value;
            jetpackAcceleration = ((Vector3Attribute)simulation.EntityManager.Entities["player"].Attributes["jetpackAcceleration"]).Value;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            UpdatePlayer(gameTime);

            world = Matrix.CreateRotationY(time * 0.1f);

            foreach (Entity e in simulation.EntityManager.Entities.Values)
            {
                int dt = gameTime.ElapsedGameTime.Milliseconds;
                UpdateEntity(e, ((float)dt)/1000.0f);
            }

            base.Update(gameTime);
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000f;

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState(PlayerIndex.One);

            bool a_pressed =
                gamePadState.Buttons.A == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Space);

            if (a_pressed && jetpackSpeed.Length() < maxJetpackSpeed)
                jetpackSpeed += jetpackAcceleration * dt;

            // graviation
            if (playerIsland == null)
                jetpackSpeed += gravityAcceleration * dt;
            else
                jetpackSpeed = Vector3.Zero;

            playerPosition += jetpackSpeed * dt;

            // moving
            playerPosition.X += GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X * playerXAxisMultiplier;
            playerPosition.Z -= GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y * playerZAxisMultiplier;

            ((Vector3Attribute)simulation.EntityManager.Entities["player"].Attributes["position"]).Value = playerPosition;
        }

        protected void UpdateEntity(Entity e, float dt)
        {
            if(e.Name.StartsWith("island"))
            {
                // read out attributes
                Vector3Attribute pos = e.Attributes["position"] as Vector3Attribute;
                Vector3Attribute vel = e.Attributes["velocity"] as Vector3Attribute;
                Vector3Attribute acc = e.Attributes["acceleration"] as Vector3Attribute;
                Vector3 v = vel.Value;

                // first force contribution: random               
                Vector3 a = new Vector3(
                    (float)rand.NextDouble()-0.5f,
                    0.0f,
                    (float)rand.NextDouble()-0.5f
                ) * islandRandomStrength;

                Vector3 islandXZ = pos.Value;
                islandXZ.Y = 0;
                bool collided = false;

                foreach(Entity pillar in pillars)
                {
                    Vector3 pillarXZ = (pillar.Attributes["position"] as Vector3Attribute).Value;
                    pillarXZ.Y = 0;
                    Vector3 dist = pillarXZ - islandXZ;
                    Vector3 pillarContribution;

                    // collision test
                    if (dist.Length() > pillarIslandCollisionRadius)
                    {
                        // no collision with this pillar
                        pillarContribution = dist;
                        pillarContribution *= pillarContribution.Length() * pillarAttraction;
                    }
                    else
                    {
                        // island collided with this pillar
                        pillarContribution = -dist * pillarRepulsion;// *(pillarIslandCollisionRadius - dist.Length()) * 10.0f;
                        if ((e.Attributes["collisionCount"] as IntAttribute).Value == 0)
                        {
                            // perform elastic collision if its the first time
                            
                            v = -v * (1.0f - pillarElasticity);
                            //Console.WriteLine("switching dir " + (e.Attributes["collisionCount"] as IntAttribute).Value);// + " " + rand.NextDouble());
                        }
                        else
                        {
                            // in this case, the island is stuck. try gradually increasing
                            // the opposing force until the island manages to escape.
                            
                            pillarContribution *= (e.Attributes["collisionCount"] as IntAttribute).Value;
                            //Console.WriteLine("contrib " + pillarContribution);
                        }
                        collided = true;
                    }
                    a += pillarContribution;
                }

                if (!collided)
                    (e.Attributes["collisionCount"] as IntAttribute).Value = 0;
                else
                    (e.Attributes["collisionCount"] as IntAttribute).Value++;

                // compute final acceleration
                acc.Value = a;

                // compute final velocity
                v = (v + dt * acc.Value) * (1.0f - islandDamping);
                float velocityLength = v.Length();
                if(velocityLength > islandMaxVelocity) {
                    v *= islandMaxVelocity / velocityLength;
                }
                vel.Value = v;

                // compute final position
                pos.Value = pos.Value + dt * vel.Value;
            }
        }

        protected void Draw(GameTime gameTime, Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Look up the effect, and set effect parameters on it. This sample
                    // assumes the model will only be using BasicEffect, but a more robust
                    // implementation would probably want to handle custom effects as well.
                    BasicEffect effect = (BasicEffect)part.Effect;

                    effect.EnableDefaultLighting();

                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;

                    // Set the graphics device to use our vertex declaration,
                    // vertex buffer, and index buffer.
                    GraphicsDevice device = effect.GraphicsDevice;

                    device.VertexDeclaration = part.VertexDeclaration;

                    device.Vertices[0].SetSource(mesh.VertexBuffer, 0,
                                                 part.VertexStride);

                    device.Indices = mesh.IndexBuffer;

                    // Begin the effect, and loop over all the effect passes.
                    effect.Begin();

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Begin();

                        // Draw the geometry.
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     part.BaseVertex, 0, part.NumVertices,
                                                     part.StartIndex, part.PrimitiveCount);

                        pass.End();
                    }

                    effect.End();
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            //Draw(gameTime, islandPrimitive);
            //world = Matrix.Identity;
            //world = Matrix.Identity;
            //Draw(gameTime, lavaPrimitive);
            //Draw(gameTime, pillarPrimitive);
            //Draw(gameTime, playerPrimitive);

            foreach (Entity e in simulation.EntityManager.Entities.Values)
            {
                if (!e.Attributes.ContainsKey("mesh") ||
                    !e.Attributes.ContainsKey("position") ||
                    !e.Attributes.ContainsKey("scale"))
                {
                    continue;
                }

                MeshAttribute mesh = e.Attributes["mesh"] as MeshAttribute;
                Vector3Attribute position = e.Attributes["position"] as Vector3Attribute;
                Vector3Attribute scale = e.Attributes["scale"] as Vector3Attribute;
                if (mesh != null && position != null && scale != null)
                {
                    world = Matrix.CreateScale(scale.Value) * Matrix.CreateTranslation(position.Value);

                    Draw(gameTime, mesh.Model);
                }
            }

            base.Draw(gameTime);
        }
    }
}
