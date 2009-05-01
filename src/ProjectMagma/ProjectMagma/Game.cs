using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#if !XBOX && DEBUG
using xWinFormsLib;
#endif

using ProjectMagma.Simulation;
using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Simulation.Collision;

using ProjectMagma.Renderer.Interface;

using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;
using System.Xml.Serialization;

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

        private List<RobotInfo> robots;
        private List<LevelInfo> levels;

        private Menu menu;
        private Entity currentCamera;

        private Settings settings = new Settings();
        
        private Simulation.Simulation simulation;
        private Renderer.Renderer renderer;

        /// <summary>
        /// assuming that the base class class update->draw->update->draw etc. the
        /// profiler will begin a frame at the beginning of the udpate methode and
        /// end the frame at the end of the draw methode
        /// </summary>
        private Profiler.Profiler profiler;

#if !XBOX && DEBUG
        private FormCollection formCollection;
        private ManagementForm managementForm;
#endif

        private static Game instance;

        StorageDevice device;
        bool storageAvailable = false;
        IAsyncResult storageSelectionResult;

        // framecounter
        private SpriteFont font;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0;
        private static int numFrames = 0;
        private static double totalMilliSeconds = 0;

        private SimulationThread simulationThread;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            menu = Menu.Instance;

            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 15.0);
            graphics.SynchronizeWithVerticalRetrace = false; 
            graphics.ApplyChanges();
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Window.Title = "Project Magma";
            Content.RootDirectory = "Content";

            // needed to show Guide, which is needed for storage, which is needed for saving stuff
            this.Components.Add(new GamerServicesComponent(this));
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
#if XBOX
            System.Threading.Thread.CurrentThread.SetProcessorAffinity(4);
#endif

            using (Game game = new Game())
            {
                Game.instance = game;

                game.Run();
            }

            Game.instance = null;
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
            ///
            // TODO: load settings from storage, save at game close
            // 

//            GraphicsDevice.RenderState.MultiSampleAntiAlias = true;
            //            GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples;

#if !XBOX && DEBUG
            // create the gui system
            formCollection = new FormCollection(this.Window, Services, ref graphics);
            managementForm = new ManagementForm(formCollection);
#endif

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // initialize renderer
            renderer = new Renderer.Renderer(Content, GraphicsDevice);

            // load level infos
            levels = Content.Load<List<LevelInfo>>("Level/LevelInfo");
            // load list of available robots
            robots = Content.Load<List<RobotInfo>>("Level/RobotInfo");

            // initialize simulation
            LoadLevel(levels[0].FileName);

#if DEBUG
            // set default player
            Entity player1 = new Entity("player1");
            player1.AddIntAttribute("game_pad_index", 0);
            player1.AddStringAttribute("robot_entity", robots[0].Entity);
            player1.AddStringAttribute("player_name", robots[0].Name);

            // set default player
            Entity player2 = new Entity("player2");
            player2.AddIntAttribute("game_pad_index", 1);
            player2.AddStringAttribute("robot_entity", robots[1].Entity);
            player2.AddStringAttribute("player_name", robots[1].Name);

            RendererUpdateQueue q = simulation.AddPlayers(new Entity[] { player1, player2 });
            renderer.AddUpdateQueue(q);
#endif

#if !XBOX && DEBUG
            managementForm.BuildForm();
#endif

            // load menu
            menu.LoadContent();

            // preload sounds
            Game.Instance.Content.Load<SoundEffect>("Sounds/gong1");
            Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/sword-clash");
            Game.Instance.Content.Load<SoundEffect>("Sounds/death");

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            // play that funky musik white boy
            MediaPlayer.Play(Game.Instance.Content.Load<Song>("Sounds/music"));
            MediaPlayer.Volume = MusicVolume;

            MediaPlayer.IsMuted = true;

            // get storage device
            storageSelectionResult = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);

            // open menu
#if !DEBUG
            menu.Open();
#endif

            this.profiler = null;
#if PROFILING
            this.profiler = ProjectMagma.Profiler.Profiler.CreateProfiler("main_profiler");
#endif

            font = Game.Instance.Content.Load<SpriteFont>("Sprites/HUD/HUDFont");
        }

        /// <summary>
        /// initializes a new simulation using the level provided
        /// </summary>
        /// <param name="level"></param>
        public void LoadLevel(String level)
        {
            // reset old simulation
            if (simulation != null)
            {
                simulation.Close();                
            }
            if (simulationThread != null)
            {
                simulationThread.Abort();
            }

            // init simulation
            simulation = new ProjectMagma.Simulation.Simulation();
            RendererUpdateQueue q = simulation.Initialize(Content, "Level/TestLevel");
            simulationThread = new SimulationThread(simulation, renderer);
            renderer.AddUpdateQueue(q);

            // set camera
            currentCamera = simulation.EntityManager["camera1"];

            RecomputeLavaTemperature();
        }

        void RecomputeLavaTemperature()
        {
            /*List<Renderer.Renderable> pillars = new List<Renderer.Renderable>();
            Renderer.Renderable lava;

            foreach (Entity entity in simulation.PillarManager)
            {
                if (entity.HasString("type") && entity.GetString("type") == "lava")
                    lava = (entity.GetProperty("render") as ModelRenderProperty).Renderable;
                if (entity.HasString("type") && entity.GetString("type") == "pillar")
                    pillars.Add((entity.GetProperty("render") as ModelRenderProperty).Renderable);
            }*/
            //System.Console.WriteLine("blah");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            simulation.Close();
            simulationThread.Abort();

#if !XBOX && DEBUG            
            formCollection.Dispose();
#endif

            MediaPlayer.Stop();

            profiler.Write(device, Window.Title, "profiling.txt");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // get storage device as soon as selected
            if (!storageAvailable && storageSelectionResult.IsCompleted)
            {
                device = Guide.EndShowStorageDeviceSelector(storageSelectionResult);
                storageAvailable = true;
                LoadSettings();
            }

            profiler.TryEndFrame();
            profiler.BeginFrame();

            // wait with normal update until storage available
            if(storageAvailable)
            {
                profiler.BeginSection("update");
            
                // fullscreen
                if(Keyboard.GetState().IsKeyDown(Keys.Enter)
                    && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
                {
                    graphics.IsFullScreen = !this.graphics.IsFullScreen;
                    graphics.ApplyChanges();
                }

#if !XBOX && DEBUG            	
                profiler.BeginSection("formcollection_update");
                // update the user interface
                formCollection.Update(gameTime);
            	profiler.EndSection("formcollection_update");
#endif
                simulationThread.Start();

                simulationThread.Join();
                //simulation.Update();

                

                // update menu
                menu.Update(gameTime);

                // update all GameComponents registered
                profiler.BeginSection("base_update");
                base.Update(gameTime);
                profiler.EndSection("base_update");
                
                profiler.EndSection("update");
            }

            renderer.Update(gameTime);
        }

        private void DrawFrameCounter(GameTime gameTime)
        {
            spriteBatch.Begin();

            numFrames++;
            totalMilliSeconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            // only start after 2 sec "warmup"
            if (totalMilliSeconds > 2000)
            {
                float fps = (float)(1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (fps > maxFPS)
                    maxFPS = fps;
                if (fps < minFPS)
                    minFPS = fps;
                spriteBatch.DrawString(
                    font,
                    String.Format("{0:000.0} fps", fps) + " " +
                    String.Format("{0:00.0} avg", (1000.0f * numFrames / totalMilliSeconds)) + " " +
                    String.Format("{0:00.0} min", minFPS) + " " +
                    String.Format("{0:00.0} max", maxFPS),
                    new Vector2(GraphicsDevice.Viewport.Width / 2 - 150, 5), Color.Silver
                );
            }

            spriteBatch.End();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            profiler.TryBeginFrame();
            profiler.BeginSection("draw");

#if !XBOX && DEBUG            
            formCollection.Render();
#endif

            renderer.Render(gameTime);

            // will apply effect such as bloom
            base.Draw(gameTime);

            // draw stuff which should not be filtered
            menu.Draw(gameTime);

#if !XBOX && DEBUG            
            formCollection.Draw();
#endif
            DrawFrameCounter(gameTime);

            profiler.EndSection("draw");
            profiler.EndFrame();
        }

        public Vector3 CameraPosition
        {
            //get { return currentCamera.GetVector3("position"); }
            get { return EyePosition; } // very redundant
        }

        public Matrix View
        {
            //get { return currentCamera.GetMatrix("view"); }
            get
            {
                return Matrix.CreateLookAt(
                    EyePosition,
                    new Vector3(0, 180, 0),
                    new Vector3(0, 1, 0)
                );
            }
        }

        public Matrix Projection
        {
            //get { return currentCamera.GetMatrix("projection"); 
            get
            {
                Viewport viewport = Game.Instance.GraphicsDevice.Viewport;
                float aspectRatio = (float)viewport.Width / (float)viewport.Height;

                // compute matrix
                return Matrix.CreatePerspectiveFieldOfView
                (
                    MathHelper.ToRadians(33.0f),
                    16.0f/9.0f,
                    1,
                    10000
                );
            }
        }

        public Vector3 EyePosition
        {
            //get
            //{
            //    return currentCamera.GetVector3("position");
            //}
            get { return new Vector3(0, 420, 1065); }
        }

        public float EffectsVolume
        {
            get { return settings.effectsVolume; }
            set { settings.effectsVolume = value; }
        }

        public float MusicVolume
        {
            get { return settings.musicVolume; }
            set { settings.musicVolume = value; }
        }

        public List<RobotInfo> Robots
        {
            get { return robots; }
        }

        public List<LevelInfo> Levels
        {
            get { return levels; }
        }

#if !XBOX && DEBUG
        public ManagementForm ManagementForm
        {
            get
            {
                return managementForm;
            }
        }
#endif

        public ProjectMagma.Simulation.Simulation Simulation
        {
            get { return simulation; }
        }

        public ProjectMagma.Renderer.Renderer Renderer
        {
            get { return renderer; }
        }

        public void SaveSettings()
        {
            // Open a storage container.StorageContainer container =
            StorageContainer container = device.OpenContainer(Window.Title);

            // Get the path of the save game.
            string filename = Path.Combine(container.Path, "settings.sav");

            // Open the file, creating it if necessary.
            using (FileStream stream = File.Open(filename, FileMode.Create))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(stream, settings);
            }

            // Dispose the container, to commit changes.
            container.Dispose();
        }

        public void LoadSettings()
        {
            // Open a storage container.StorageContainer container =
            StorageContainer container = device.OpenContainer(Window.Title);

            // Get the path of the save game.
            string filename = Path.Combine(container.Path, "settings.sav");

            // Open the file, creating it if necessary.
            if (File.Exists(filename))
            {
                using (FileStream stream = File.Open(filename, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    settings = (Settings) serializer.Deserialize(stream);
                }
            }

            // Dispose the container, to commit changes.
            container.Dispose();
        }

        public class Settings
        {
            public float effectsVolume = 0.2f;
            public float musicVolume = 0.1f;
        }

        public ProjectMagma.Profiler.Profiler Profiler
        {
            get { return profiler; }
        }
    }

    public delegate void IntervalExecutionAction(int times);
    public delegate void PushBackFinishedHandler();
}
