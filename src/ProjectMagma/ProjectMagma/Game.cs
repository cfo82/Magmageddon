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

        protected EntityManager entityManager;
        private PillarManager pillarManager;
        private IslandManager islandManager;

        private static Game instance;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

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

        public static Game Instance
        {
            get
            {
                return Game.instance;
            }
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

            foreach (EntityData entityData in levelData.entities)
            {
                entityManager.AddEntity(Content, entityData);
            }
           
            foreach (Entity e in entityManager)
            {
                if (e.Name.StartsWith("island"))
                {
                    e.AddAttribute("collisionCount", "int", "0");
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

            player.AddAttribute("energy", "int", "100");
            player.AddAttribute("health", "int", "100");            
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

            if (a_pressed)
            {
                jetpackSpeed += jetpackAcceleration * dt;
            }
            if (jetpackSpeed.Length() > maxJetpackSpeed)
            {
                jetpackSpeed.Normalize();
                jetpackSpeed *= maxJetpackSpeed;
            }

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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach (Entity e in entityManager)
            {
                e.OnDraw(gameTime);
            }

            base.Draw(gameTime);
        }

        public PillarManager PillarManager
        {
            get
            {
                return pillarManager;
            }
        }

        public Matrix View
        {
            get
            {
                return view;
            }
        }

        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }
    }
}
