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
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        private Vector3 playerPosition;
        private Vector3 jetpackAcceleration;
        private Vector3 jetpackSpeed = new Vector3(0, 0, 0);
        private Vector3 gravityAcceleration = new Vector3(0, -120f, 0);
        private float maxJetpackSpeed = 100f;
        private Entity playerIsland = null;

        private int playerXAxisMultiplier = 1;
        private int playerZAxisMultiplier = 2;

        protected AttributeTemplateManager attributeTemplateManager;
        protected EntityManager entityManager;
        private PillarManager pillarManager;
        private IslandManager islandManager;

        private Random rand;
        private static Game instance;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            rand = new Random(485394);

            attributeTemplateManager = new AttributeTemplateManager();
            entityManager = new EntityManager();
            pillarManager = new PillarManager();
            islandManager = new IslandManager();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            // we can use hardware threads 1, 3, 4, 5
            //  1 is running on core 1 alongside something reserved to xna
            //  3 same thing on core 2
            //  4, 5 run on core 3 and are freely available for us

            // I am not sure if we have to do the rendering using the thread that created
            // the direct3d device. This would be something to clarify in the future. As for
            // now we should keep to rendering on the main thread and move everything else to
            // the other cores.

            // another thing to clarify would be on which thread the Main-methode is running (
            // and if the SetProcessorAffinity works... It could very well be that this thread is
            // already locked to some hardware thread.
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 1 });
#endif

            using (Game game = new Game())
            {
                Game.instance = game;

                game.Run();
                
                Game.instance = null;
            }
        }

        public static Game GetInstance()
        {
            return Game.instance;
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
            
            LevelData levelData = Content.Load<LevelData>("Level/TestLevel");

            foreach (AttributeTemplateData attributeTemplateData in levelData.attributeTemplates)
            {
                attributeTemplateManager.AddAttributeTemplate(attributeTemplateData);
            }
            foreach (EntityData entityData in levelData.entities)
            {
                entityManager.AddEntity(Content, entityData);
            }

            attributeTemplateManager.AddAttributeTemplate("General.CollisionCount", typeof(IntAttribute).FullName);
            attributeTemplateManager.AddAttributeTemplate("General.PlayerResource", typeof(IntAttribute).FullName);
           
            foreach (Entity e in entityManager)
            {
                if (e.Name.StartsWith("island"))
                {
                    e.AddAttribute(Content, "collisionCount", "General.CollisionCount", "0");
                    e.AddProperty("controller", new IslandControllerProperty());
                    islandManager.AddIsland(e);
                }
            }

            foreach (Entity e in entityManager)
            {
                if (e.Name.StartsWith("pillar"))
                {
                    pillarManager.AddPillar(e);
                }
            }         

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            view = Matrix.CreateLookAt(new Vector3(0, 850, 1400),
                                       new Vector3(0, 150, 0), Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4/2.0f,
                                                             aspectRatio, 10, 10000);
            // TODO: use this.Content to load your game content here

            Entity player = entityManager["player"];

            playerPosition = player.GetVector3("position");
            jetpackAcceleration = player.GetVector3("jetpackAcceleration");

            player.AddAttribute(Content, "energy", "General.PlayerResource", "100");
            player.AddAttribute(Content, "health", "General.PlayerResource", "100");            
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

            foreach (Entity e in entityManager)
            {
                e.OnUpdate(gameTime);
            }

            UpdatePlayer(gameTime);

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

            entityManager["player"].SetVector3("position", playerPosition);
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

            foreach (Entity e in entityManager)
            {
                if (!e.HasAttribute("mesh") || (e.Attributes["mesh"] as MeshAttribute) == null ||
                    !e.HasAttribute("position") || !e.IsVector3("position") ||
                    (e.HasAttribute("scale") && !e.IsVector3("scale"))
                    )
                {
                    continue;
                }

                Model model = (e.Attributes["mesh"] as MeshAttribute).Model;
                Vector3 position = e.GetVector3("position");
                Vector3 scale = new Vector3(1, 1, 1);
                if (e.HasAttribute("scale"))
                {
                    scale = e.GetVector3("scale");
                }

                world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                Draw(gameTime, model);
            }

            GraphicsDevice.RenderState.PointSize = 110;
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.PointList, new VertexPositionColor[] {
                         new VertexPositionColor(playerPosition, Color.Red)}, 0, 1);

            base.Draw(gameTime);
        }

        public AttributeTemplateManager AttributeTemplateManager
        {
            get
            {
                return attributeTemplateManager;
            }
        }

        public PillarManager PillarManager
        {
            get
            {
                return pillarManager;
            }
        }
    }
}
