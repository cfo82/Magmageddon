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

        private EntityManager entityManager;
        private PillarManager pillarManager;
        private IslandManager islandManager;
        private IceSpikeManager iceSpikeManager;

        private static Game instance;

        private Effect testEffect;
        private SpriteFont HUDFont;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            RenderProperty.device = graphics.GraphicsDevice;
            Window.Title = "Project Magma";
            Content.RootDirectory = "Content";

            entityManager = new EntityManager();

            // changed by dpk on mar 12, on advice by obi
            pillarManager = new PillarManager();
            islandManager = new IslandManager();
            iceSpikeManager = new IceSpikeManager();
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

            // another thing to clarify would be on which thread the main method is running 
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
            HUDFont = Content.Load<SpriteFont>("HUDFont");

            LevelData levelData = Content.Load<LevelData>("Level/TestLevel");

            //testEffect = Content.Load<Effect>("Effects/TestEffect");

            foreach (EntityData entityData in levelData.entities)
            {
                entityManager.Add(Content, entityData);
            }

            int gi = 0;
            foreach (Entity e in entityManager)
            {
                if (e.Name.StartsWith("island"))
                {
                    islandManager.Add(e);
                }
                else
                    if (e.Name.StartsWith("pillar"))
                    {
                        pillarManager.Add(e);
                    }
                    else
                        if (e.Name.StartsWith("player"))
                        {
                            e.AddAttribute("gamePadIndex", "int", "" + (gi++));
                        }
            }

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            view = Matrix.CreateLookAt(new Vector3(0, 850, 1400),
                                       new Vector3(0, 150, 0), Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4/2.0f,
                                                             aspectRatio, 10, 10000);       
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

            base.Update(gameTime);
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

            // draw infos about state
            spriteBatch.Begin();
            int pos = 0;
            foreach (Entity e in entityManager)
            {
                if (e.Name.StartsWith("player"))
                {
                    spriteBatch.DrawString(HUDFont, e.Name + "; health: " + e.GetInt("health") + ", energy: " + e.GetInt("energy") + ", fuel: " + e.GetInt("fuel"),
                        new Vector2(0, pos), Color.White);
                    pos += 15;
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public PillarManager PillarManager
        {
            get
            {
                return pillarManager;
            }
        }

        public IslandManager IslandManager
        {
            get
            {
                return islandManager;
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

        public EntityManager EntityManager
        {
            get
            {
                return entityManager;
            }
        }

    }
}
