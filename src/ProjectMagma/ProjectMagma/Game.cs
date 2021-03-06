#define ALWAYS_FOUR_PLAYERS
#define TEST_RELEASE

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
        private static readonly float DefaultEffectsVolume = 0.5f;
        private static readonly float DefaultMusicVolume = 0.2f;

        private SimulationThread simulationThread;

        private CrashDebugger crashDebugger;
        
        private GlobalClock globalClock;
        private AudioPlayer audioPlayer;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            menu = Menu.Instance;
            wrappedContentManager = new WrappedContentManager(Content);

            // FIXME:
            //   If we do 720p with our three rendertargets and the depth/stencil buffer
            //   we'll use (in case we can use 32bpp rendertargets) 16*720*1280 bytes
            //   which results in 14.0625MB. The Xenon EDRAM is 10MB therefore the
            //   chip will do at least two tiles. 
            //   if we go down to a resolution of 1120:630 we'll need less than 
            //   10MB of memory for our backbuffers. Thus only one tile will be needed.
            // We should try two things:
            //   *) Check if the lower resolution leads to a speedup which is worth the
            //      potentially lower quality
            //   *) Check all settings with Multisampling. (May not be possible with MRTs?)
            //      Maybe we could do it without MRT's?


            this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 15.0);
            const float multiplier = 1.0f;
            graphics.PreferredBackBufferWidth = (int)(1280 * multiplier);
            graphics.PreferredBackBufferHeight = (int)(720 * multiplier);
            graphics.ApplyChanges();

            Window.Title = "Project Magma";
#if XDK
            ContentManager.RootDirectory = "_XBLA_Content";
#else
            ContentManager.RootDirectory = "Content";
#endif

            // needed to show Guide, which is needed for storage, which is needed for saving stuff
            Components.Add(new GamerServicesComponent(this));

            this.globalClock = new GlobalClock();
            this.audioPlayer = new AudioPlayer();

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

        protected override void OnExiting(object sender, EventArgs args)
        {
            SaveSettings();
            base.OnExiting(sender, args);
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

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// LoadContent will be called once per game and is theF place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            SoundEffect.MasterVolume = EffectsVolume;
            MediaPlayer.Volume = MusicVolume;

            crashDebugger = new CrashDebugger(GraphicsDevice, ContentManager, "Fonts/kootenay20", "dpk@student.ethz.ch");

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


#if DEBUG || TEST_RELEASE
            // initialize simulation
            LoadLevel("Level/Instances/TestLevel/Simulation", "Level/Instances/TestLevel/Renderer");
            //LoadLevel("Level/Instances/4vs4/Simulation", "Level/Instances/4vs4/Renderer");
            //LoadLevel("Level/Instances/StaircaseOfDoom/Simulation", "Level/Instances/StaircaseOfDoom/Renderer");
            //LoadLevel("Level/Instances/Stack/Simulation", "Level/Instances/Stack/Renderer");
            //LoadLevel("Level/Instances/4Noobs/Simulation", "Level/Instances/4Noobs/Renderer");

            // set default player
            Entity player1 = new Entity("player1");
            player1.AddIntAttribute(CommonNames.GamePadIndex, 0);
            player1.AddIntAttribute(CommonNames.Lives, 100);
            player1.AddStringAttribute("robot_entity", robots[0].Entity);
            player1.AddStringAttribute(CommonNames.PlayerName, robots[0].Name);

            // set default player
            Entity player2 = new Entity("player2");
            player2.AddIntAttribute(CommonNames.GamePadIndex, 1);
            player2.AddIntAttribute(CommonNames.Lives, 100);
            player2.AddStringAttribute("robot_entity", robots[1].Entity);
            player2.AddStringAttribute(CommonNames.PlayerName, robots[1].Name);
    #if ALWAYS_FOUR_PLAYERS
            // set default player
            Entity player3 = new Entity("player3");
            player3.AddIntAttribute(CommonNames.GamePadIndex, 2);
            player3.AddIntAttribute(CommonNames.Lives, 100);
            player3.AddStringAttribute("robot_entity", robots[2].Entity);
            player3.AddStringAttribute(CommonNames.PlayerName, robots[2].Name);
            
            // set default player
            Entity player4 = new Entity("player4");
            player4.AddIntAttribute(CommonNames.GamePadIndex, 3);
            player4.AddIntAttribute(CommonNames.Lives, 100);
            player4.AddStringAttribute("robot_entity", robots[3].Entity);
            player4.AddStringAttribute(CommonNames.PlayerName, robots[3].Name);

            AddPlayers(new Entity[] { player1, player2, player3, player4 });
#else
            AddPlayers(new Entity[] { player1, player2 });
    #endif
#else
            // initialize simulation
            LoadLevel("Level/Instances/MenuLevel/Simulation", "Level/Instances/MenuLevel/Renderer");
            //LoadLevel("Level/Instances/StaircaseOfDoom/Simulation", "Level/Instances/StaircaseOfDoom/Renderer");
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
            Game.Instance.ContentManager.Load<Texture2D>("Textures/Sfx/respawnspot");
            Game.Instance.ContentManager.Load<MagmaModel>("Models/Sfx/IceSpike");

            // preload sounds
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/gong1");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/punch2");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/hit2");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/sword-clash");
            Game.Instance.ContentManager.Load<SoundEffect>("Sounds/death");
            Game.Instance.ContentManager.Load<SoundEffect>(Menu.OkSound);
            Game.Instance.ContentManager.Load<SoundEffect>(Menu.ChangeSound);
            Game.Instance.ContentManager.Load<SoundEffect>(Menu.BackSound);

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            // play that funky musik white boy
            //MediaPlayer.Play(Game.Instance.ContentManager.Load<Song>("Music/background_janick"));

            // get storage device => moved to the update loop
            //storageSelectionResult = Guide.BeginShowStorageDeviceSelector(PlayerIndex.One, null, null);

            // open menu
#if !DEBUG && !PROFILE && !TEST_RELEASE
            menu.Open();
#endif

            this.profiler = null;
#if PROFILING
            this.profiler = ProjectMagma.Profiler.Profiler.CreateProfiler(Game.Instance.ContentManager, "rendering_profiler");
#endif

            fpsFont = Game.Instance.ContentManager.Load<SpriteFont>("Fonts/fps");
        }

        /// <summary>
        /// initializes a new simulation using the level provided
        /// </summary>
        /// <param name="level"></param>
        public void LoadLevel(
            string simulationLevel,
            string rendererLevel
        )
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
            RendererUpdateQueue q = simulation.Initialize(ContentManager, simulationLevel, rendererLevel, globalClock.PausableMilliseconds);
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

                simulationThread.Profiler.Write(device, Window.Title, "profiling_simulation.txt");
                profiler.Write(device, Window.Title, "profiling_renderer.txt");

#if !XBOX
                Debug.Assert(simulationThread.Thread.ThreadState == System.Threading.ThreadState.Stopped);
#endif
#if !DEBUG
            }
            catch (Exception)
            {
                
            }
#endif
        }

        bool paused = true;

        public void Pause()
        {
            simulationThread.Join();
            AudioPlayer.PauseAll();
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
            AudioPlayer.ResumeAll();
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
            if (CrashDebugger.Crashed)
            {
                if (!Paused)
                {
                    Pause();
                    RendererUpdateQueue q = simulation.Close();
                    renderer.AddUpdateQueue(q);
                    simulationThread.Abort();
                }

                crashDebugger.Update(GraphicsDevice);

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    Exit();
                }
                return;
            }

#if XBOX && !DEBUG
            try
            {
#endif
                // get storage device => moved
                if (!Guide.IsVisible && storageSelectionResult == null)
                {
                    try
                    {
                        storageSelectionResult = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
                    }
                    catch (GuideAlreadyVisibleException)
                    {
                        // FIXME. see also
                        //    http://forums.xna.com/forums/p/19874/103843.aspx
                        //    http://blog.nickgravelyn.com/2009/07/storage-device-management-20/
                    }
                }

                // get storage device as soon as selected
                if (storageSelectionResult != null && !storageAvailable && storageSelectionResult.IsCompleted)
                {
                    device = StorageDevice.EndShowSelector(storageSelectionResult);
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

                    profiler.HandleInput(gameTime);

                    //simulationThread.Join();

                    // update menu
                    menu.Update(gameTime);

                    // update all GameComponents registered
                    profiler.BeginSection("base_update");
                    base.Update(gameTime);
                    profiler.EndSection("base_update");

                    profiler.EndSection("update");
                }
#if XBOX && !DEBUG
            }
            catch (Exception e)
            {
                crashDebugger.Crash(e);
            }
#endif
        }

        float[] lastFPS = new float[1000];
        bool[] lastFPSValid = new bool[1000];
        int currentFPS = 0;

        private void DrawFrameCounter(GameTime gameTime)
        {
            spriteBatch.Begin();

            numFrames++;
            totalMilliSeconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            // only start after 2 sec "warm-up"
            if (totalMilliSeconds > 10000)
            {
                Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;

                float fps = (float)(1000f / gameTime.ElapsedGameTime.TotalMilliseconds);
                if (fps > maxFPS)
                    maxFPS = fps;
                if (fps < minFPS)
                    minFPS = fps;

                lastFPS[currentFPS] = fps;
                lastFPSValid[currentFPS] = true;
                currentFPS = (currentFPS + 1) % lastFPS.Length;

                float framesCount = 0;
                float framesLT30Count = 0;
                for (int i = 0; i < lastFPS.Length; ++i)
                {
                    if (lastFPSValid[i])
                    {
                        ++framesCount;
                        if (lastFPS[i] < 30)
                            ++framesLT30Count;
                    }
                }
                if (framesCount == 0)
                {
                    framesCount = 1;
                    framesLT30Count = 0;
                }

                String renderingText = string.Format("rendering\n- cur: {0:000.0}\n- avg: {1:00.0}\n- min: {2:00.0}\n- <30fps: {3:00.00}%", fps, (1000.0f * numFrames / totalMilliSeconds), minFPS, (framesLT30Count / framesCount) * 100.0f);
                String simulationText = string.Format("simulation\n- cur: {0:000.0}\n- avg: {1:00.0}",
                    (simulationThread != null ? simulationThread.Sps : 0),
                    (simulationThread != null ? simulationThread.AvgSps : 0));

                Vector2 renderingTextSize = fpsFont.MeasureString(renderingText);
                Vector2 simulationTextSize = fpsFont.MeasureString(simulationText);

                spriteBatch.DrawString(
                    fpsFont,
                    renderingText,
                    new Vector2(titleSafeArea.X + 4, titleSafeArea.Y + titleSafeArea.Height / 2.0f - renderingTextSize.Y / 2.0f),
                    Color.White * 0.7f
                    );
                spriteBatch.DrawString(
                   fpsFont,
                   simulationText,
                   new Vector2(titleSafeArea.X + titleSafeArea.Width - simulationTextSize.X - 4, titleSafeArea.Y + titleSafeArea.Height / 2.0f - simulationTextSize.Y / 2.0f),
                   Color.White * 0.7f
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
            if (crashDebugger.Crashed)
            {
                crashDebugger.Draw(GraphicsDevice);
                return;
            }

/*#if XBOX && !DEBUG
            try
            {
#endif*/
                profiler.TryBeginFrame();
                profiler.BeginSection("draw");

                renderer.Render();

                // will apply effect such as bloom
                base.Draw(gameTime);

                // draw stuff which should not be filtered
                menu.Draw(gameTime);

                // let the profiler draw its overlay
                profiler.DrawOverlay(GraphicsDevice);

#if DEBUG || TEST_RELEASE
                DrawFrameCounter(gameTime);
#endif

                profiler.EndSection("draw");
                profiler.EndFrame();
/*#if XBOX && !DEBUG
            }
            catch (Exception e)
            {
                // make sure to reset the render target (else it may happen that present fails)
                // so that we can correctly display the crashDebugger!
                renderer.Device.SetRenderTarget(null);

                crashDebugger.Crash(e);
            }
#endif*/
        }

        #region Stuff to be moved by janick!
        // TODO: move things to their place!!

        public float EffectsVolume
        {
            get { return settings.effectsVolume; }
            set { settings.effectsVolume = value; SoundEffect.MasterVolume = value; }
        }

        public float MusicVolume
        {
            get { return settings.musicVolume; }
            set { settings.musicVolume = value; MediaPlayer.Volume = value; }
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
            IAsyncResult result = device.BeginOpenContainer(Window.Title, null, null);

            // wait for the waithandle to become signaled
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Open the file, creating it if necessary.
            using (Stream stream = container.OpenFile("settings.xml", FileMode.Create))
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
            IAsyncResult result = device.BeginOpenContainer(Window.Title, null, null);

            // wait for the waithandle to become signaled
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = device.EndOpenContainer(result);

            // Open the file, creating it if necessary.
            if (container.FileExists("settings.xml"))
            {
                using (Stream stream = container.OpenFile("settings.xml", FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    settings = (Settings)serializer.Deserialize(stream);
                    ApplySettings();
                }
            }
            else
            {
                settings.effectsVolume = DefaultEffectsVolume;
                settings.musicVolume = DefaultMusicVolume;
                ApplySettings();
            }

            // Dispose the container, to commit changes.
            container.Dispose();
        }

        private void ApplySettings()
        {
            EffectsVolume = settings.effectsVolume;
            MusicVolume = settings.musicVolume;
            if (simulation != null)
                { simulation.MusicSettingsLoaded(); }
        }

        public class Settings
        {
            public float effectsVolume = 0.0f;
            public float musicVolume = 0.0f;
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

        public CrashDebugger CrashDebugger
        {
            get { return crashDebugger; }
        }
        
        public GlobalClock GlobalClock
        {
        	get { return globalClock; }
        }

        public AudioPlayer AudioPlayer
        {
            get { return audioPlayer; }
        }
    }
}
