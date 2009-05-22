#define ALWAYS_FOUR_PLAYERS

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using ProjectMagma.Simulation;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Shared.Model;
using ProjectMagma.Simulation.Collision;

using ProjectMagma.Renderer.Interface;

using ProjectMagma.Bugslayer;

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
        //private Entity currentCamera;

        private Settings settings = new Settings();
        
        private Simulation.Simulation simulation;
        private Renderer.Renderer renderer;

        private WrappedContentManager wrappedContentManager;

        /// <summary>
        /// assuming that the base class class update->draw->update->draw etc. the
        /// profiler will begin a frame at the beginning of the udpate methode and
        /// end the frame at the end of the draw methode
        /// </summary>
        private Profiler.Profiler profiler;

        private static Game instance;

        StorageDevice device;
        bool storageAvailable = false;
        IAsyncResult storageSelectionResult;

        // framecounter
        private SpriteFont fpsFont;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0;
        private static int numFrames = 0;
        private static double totalMilliSeconds = 0;

        private SimulationThread simulationThread;

        private Exception exceptionThrown;
        private CrashDebugger crashDebugger;
        
        private GlobalClock globalClock;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            menu = Menu.Instance;
            wrappedContentManager = new WrappedContentManager(Content);

            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 15.0);
            graphics.SynchronizeWithVerticalRetrace = false; 
            graphics.ApplyChanges();
            const float multiplier = 1.0f;
            graphics.PreferredBackBufferWidth = (int) (1280 * multiplier);
            graphics.PreferredBackBufferHeight = (int) (720 * multiplier);

            Window.Title = "Project Magma";
            ContentManager.RootDirectory = "Content";

            // needed to show Guide, which is needed for storage, which is needed for saving stuff
            this.Components.Add(new GamerServicesComponent(this));
            this.globalClock = new GlobalClock();
        }

        public static Game Instance
        {
            get
            {
                return Game.instance;
            }
        }

        public static void RunInstance()
        {
            using (Game game = new Game())
            {
                Game.instance = game;

                game.Run();
            }

            Game.instance = null;
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
            crashDebugger = new CrashDebugger(GraphicsDevice, ContentManager);

//            GraphicsDevice.RenderState.MultiSampleAntiAlias = true;
            //            GraphicsDevice.PresentationParameters.MultiSampleType = MultiSampleType.FourSamples;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.globalClock.Start();

            // initialize renderer
            renderer = new Renderer.Renderer(ContentManager, GraphicsDevice);

            // load level infos
            levels = ContentManager.Load<List<LevelInfo>>("Level/LevelInfo");
            // load list of available robots
            robots = ContentManager.Load<List<RobotInfo>>("Level/Common/RobotInfo");

             
#if DEBUG
            // initialize simulation
            LoadLevel("Level/Instances/TestLevel/Simulation");

            // set default player
            Entity player1 = new Entity("player1");
            player1.AddIntAttribute("game_pad_index", 0);
            player1.AddIntAttribute("lives", 100);
            player1.AddStringAttribute("robot_entity", robots[0].Entity);
            player1.AddStringAttribute("player_name", robots[0].Name);

            // set default player
            Entity player2 = new Entity("player2");
            player2.AddIntAttribute("game_pad_index", 1);
            player2.AddIntAttribute("lives", 100);
            player2.AddStringAttribute("robot_entity", robots[1].Entity);
            player2.AddStringAttribute("player_name", robots[1].Name);
    #if ALWAYS_FOUR_PLAYERS
            // set default player
            Entity player3 = new Entity("player3");
            player3.AddIntAttribute("game_pad_index", 2);
            player3.AddIntAttribute("lives", 100);
            player3.AddStringAttribute("robot_entity", robots[2].Entity);
            player3.AddStringAttribute("player_name", robots[2].Name);
            
            // set default player
            Entity player4 = new Entity("player4");
            player4.AddIntAttribute("game_pad_index", 3);
            player4.AddIntAttribute("lives", 100);
            player4.AddStringAttribute("robot_entity", robots[3].Entity);
            player4.AddStringAttribute("player_name", robots[3].Name);

            AddPlayers(new Entity[] { player1, player2, player3, player4 });
    #else
            AddPlayers(new Entity[] { player1, player2 });
    #endif
#else
            // initialize simulation
            LoadLevel("Level/Instances/MenuLevel/Simulation");
#endif

            // load menu
            menu.LoadContent();

            // preload effects data
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/FireExplosion");
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceExplosion");
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/IceSpike");
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/LavaExplosion");
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/Snow");
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/ParticleSystem/Stateful/Implementations/Flamethrower");
            Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/IceSpike");
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/FireExplosion");
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/IceExplosion");
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/IceSpikeHead");
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/IceSpikeTrail");
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/LavaExplosion");
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/Snow");
            Game.Instance.ContentManager.Load<MagmaModel>("Models/Sfx/IceSpike");

            // preload sounds
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/gong1");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/punch2");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/hit2");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/sword-clash");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/death");

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            // play that funky musik white boy
            MediaPlayer.Play(Game.Instance.ContentManager.Load<Song>("Sounds/music"));
            MediaPlayer.Volume = MusicVolume;

            MediaPlayer.IsMuted = true;

            // get storage device
            storageSelectionResult = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);

            // open menu
#if !DEBUG
            menu.OpenReleaseNotes();
#endif

            this.profiler = null;
#if PROFILING
            this.profiler = ProjectMagma.Profiler.Profiler.CreateProfiler("main_profiler");
#endif

            fpsFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/fps");
        }

        /// <summary>
        /// initializes a new simulation using the level provided
        /// </summary>
        /// <param name="level"></param>
        public void LoadLevel(String level)
        {
            if (simulationThread != null)
            {
                if (!paused)
                {
                    simulationThread.Join();
                }
            }

            if (simulationThread == null)
            {
                simulationThread = new SimulationThread();
            }

            // reset old simulation
            if (simulation != null)
            {
                RendererUpdateQueue q1 = simulation.Close();
                renderer.AddUpdateQueue(q1);
            }

            // init simulation
            simulation = new ProjectMagma.Simulation.Simulation();
            RendererUpdateQueue q = simulation.Initialize(ContentManager, level, globalClock.PausableMilliseconds);
            renderer.AddUpdateQueue(q);

#if !XBOX
            Debug.Assert(
                simulationThread == null || simulationThread.Thread == null ||
                simulationThread.Thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin
                );
#endif

            simulationThread.Reinitialize(this.simulation, this.renderer);

            // set camera
            //currentCamera = simulation.EntityManager["camera1"];

            if (!paused)
            {
                simulationThread.Start();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
#if !DEBUG
            try
            {
#endif
#if !XBOX
                Debug.Assert(
                    simulationThread.Thread.ThreadState == System.Threading.ThreadState.Stopped ||
                    simulationThread.Thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin
                    );
#endif

                RendererUpdateQueue q = simulation.Close();
                renderer.AddUpdateQueue(q);
                simulationThread.Abort();

                MediaPlayer.Stop();

                profiler.Write(device, Window.Title, "profiling.txt");

#if !XBOX
                Debug.Assert(simulationThread.Thread.ThreadState == System.Threading.ThreadState.Stopped);
#endif
#if !DEBUG
            }
            catch (Exception ex) { }
#endif
        }

        bool paused = true;

        public void Pause()
        {
            simulationThread.Join();
            paused = true;
            globalClock.Pause();
            simulation.Pause();
        }

        public bool Paused
        {
            get { return paused; }
        }

        public void Resume()
        {
            simulation.Resume();
            paused = false;
            globalClock.Resume();
            simulationThread.Start();
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);

            if (paused && !menu.Active)
            {
                Resume();
            }
        }


        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            if (!paused)
            {
                Pause();
            }
        }

        public void AddPlayers(Entity[] players)
        {
            if (!paused)
            {
                Pause();
                ProjectMagma.Renderer.Interface.RendererUpdateQueue q = Game.Instance.Simulation.AddPlayers(players);
                Game.Instance.Renderer.AddUpdateQueue(q);

                Resume();
            }
            else
            {
                ProjectMagma.Renderer.Interface.RendererUpdateQueue q = Game.Instance.Simulation.AddPlayers(players);
                Game.Instance.Renderer.AddUpdateQueue(q);
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ExceptionThrown != null)
            {
                if (!Paused)
                {
                    Pause();
                    RendererUpdateQueue q = simulation.Close();
                    renderer.AddUpdateQueue(q);
                    simulationThread.Abort();
                }

                crashDebugger.SetException(ExceptionThrown);

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    Exit();
                }
                return;
            }

#if XBOX
            try
            {
#endif
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
                if (storageAvailable)
                {
                    profiler.BeginSection("update");

                    // fullscreen
                    if (Keyboard.GetState().IsKeyDown(Keys.Enter)
                        && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
                    {
                        graphics.IsFullScreen = !this.graphics.IsFullScreen;
                        graphics.ApplyChanges();
                    }

                    //simulationThread.Join();

                    // update menu
                    menu.Update(gameTime);

                    // update all GameComponents registered
                    profiler.BeginSection("base_update");
                    base.Update(gameTime);
                    profiler.EndSection("base_update");

                    profiler.EndSection("update");
                }
#if XBOX
            }
            catch (Exception e)
            {
                exceptionThrown = e;
            }
#endif
        }

        private void DrawFrameCounter(GameTime gameTime)
        {
            spriteBatch.Begin();

            numFrames++;
            totalMilliSeconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            // only start after 2 sec "warm-up"
            if (totalMilliSeconds > 2000)
            {
                float fps = (float)(1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (fps > maxFPS)
                    maxFPS = fps;
                if (fps < minFPS)
                    minFPS = fps;
                spriteBatch.DrawString(
                    fpsFont,
                    "rendering\n" +
                    String.Format("- cur: {0:000.0}", fps) + "\n" +
                    String.Format("- avg: {0:00.0}", (1000.0f * numFrames / totalMilliSeconds)) + "\n",
                    // is min/max really necessary?
                    //String.Format("- min: {0:00.0}", minFPS) + "\n" +
                    //String.Format("- max: {0:00.0}", maxFPS),
                    new Vector2(4, 7), new Color(Color.White, 0.7f)
                );
                spriteBatch.DrawString(
                   fpsFont,
                   "simulation\n" +
                   String.Format("- cur: {0:000.0}", (simulationThread != null ? simulationThread.Sps : 0)) + "\n" +
                   String.Format("- avg: {0:00.0}", (simulationThread != null ? simulationThread.AvgSps : 0)) + "\n",
                   new Vector2(GraphicsDevice.Viewport.Width-120, 7), new Color(Color.White, 0.7f)
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
            if (exceptionThrown != null)
            {
                crashDebugger.Draw(GraphicsDevice);
                return;
            }

#if XBOX
            try
            {
#endif
                profiler.TryBeginFrame();
                profiler.BeginSection("draw");

                renderer.Render();

                // will apply effect such as bloom
                base.Draw(gameTime);

                // draw stuff which should not be filtered
                menu.Draw(gameTime);

                DrawFrameCounter(gameTime);

                profiler.EndSection("draw");
                profiler.EndFrame();
#if XBOX
            }
            catch (Exception e)
            {
                exceptionThrown = e;
            }
#endif
        }

        #region Stuff to be moved by janick!
        // TODO: move things to their place!!

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
            return; 

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

        #endregion 

        public ProjectMagma.Profiler.Profiler Profiler
        {
            get { return profiler; }
        }

        public SimulationThread SimulationThread
        {
            get { return simulationThread; }
        }


        public WrappedContentManager ContentManager
        {
            get { return wrappedContentManager; }
        }

        /*public new Microsoft.Xna.Framework.Content.ContentManager Content
        {
            get
            {
                if (wrappedContentManager != null)
                {
                    throw new Exception("FUCK OFF"); 
                }
                else
                {
                    return base.Content; 
                }
            }
        }*/

        public Exception ExceptionThrown
        {
            set
            {
                lock (this)
                {
                    exceptionThrown = value;
                }
            }

            get
            {
                lock (this)
                {
                    return exceptionThrown;
                }
            }
        }
        
        public GlobalClock GlobalClock
        {
        	get { return globalClock; }
        }
    }
}
