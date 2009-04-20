using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#if !XBOX
using xWinFormsLib;
#endif

using ProjectMagma.Simulation;
using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Simulation.Collision;

using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

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
        private float effectsVolume = 0.2f;
        private float musicVolume = 0.1f;
        
        private Simulation.Simulation simulation;
        private Renderer.Renderer renderer;

        /// <summary>
        /// assuming that the base class class update->draw->update->draw etc. the
        /// profiler will begin a frame at the beginning of the udpate methode and
        /// end the frame at the end of the draw methode
        /// </summary>
        private Profiler.Profiler profiler;

        #if !XBOX
        private FormCollection formCollection;
        private ManagementForm managementForm;
        #endif

        private static Game instance;

        BloomComponent bloom;

        private Game()
        {

            graphics = new GraphicsDeviceManager(this);
            menu = Menu.Instance;

            this.IsFixedTimeStep = false;
//            graphics.SynchronizeWithVerticalRetrace = false;
//            graphics.ApplyChanges();

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Window.Title = "Project Magma";
            Content.RootDirectory = "Content";

            bloom = new BloomComponent(this);
            //Components.Add(bloom);
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
            System.Threading.Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
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

            #if !XBOX
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

            simulation.AddPlayers(new Entity[] { player1, player2 });
#endif

            #if !XBOX
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
            MediaPlayer.Volume = musicVolume;

            MediaPlayer.IsMuted = true;

            // open menu
#if !DEBUG
            menu.Open();
#endif

            this.profiler = null;
#if PROFILING
            this.profiler = ProjectMagma.Profiler.Profiler.CreateProfiler("main_profiler");
#endif
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

            // init simulation
            simulation = new ProjectMagma.Simulation.Simulation();
            simulation.Initialize(Content, "Level/TestLevel");

            // set camera
            currentCamera = simulation.EntityManager["camera1"];
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            simulation.Close();

            #if !XBOX
            formCollection.Dispose();
            #endif

            MediaPlayer.Stop();

            #if PROFILING
            profiler.Write("profiling.txt");
            #endif
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            profiler.TryEndFrame();
            profiler.BeginFrame();
            profiler.BeginSection("update");

            // fullscreen
            if(Keyboard.GetState().IsKeyDown(Keys.Enter)
                && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                graphics.IsFullScreen = !this.graphics.IsFullScreen;
                graphics.ApplyChanges();
            }

            #if !XBOX
            // update the user interface
            profiler.BeginSection("formcollection_update");
            formCollection.Update(gameTime);
            profiler.EndSection("formcollection_update");
            #endif

            simulation.Update(gameTime);

            // update menu
            menu.Update(gameTime);

            // update all GameComponents registered
            profiler.BeginSection("base_update");
            base.Update(gameTime);
            profiler.EndSection("base_update");

            profiler.EndSection("update");
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            profiler.TryBeginFrame();
            profiler.BeginSection("draw");

            #if !XBOX
            formCollection.Render();
            #endif

            // TODO: small hack until dominik has added the bloom component...

            renderer.Render(gameTime);

            Components.Remove(bloom); // prevent bloom here

            // will apply effect such as bloom
            base.Draw(gameTime);

            //Components.Add(bloom); // add it again in order for updates...

            // draw stuff which should not be filtered
            menu.Draw(gameTime);

            #if !XBOX
            formCollection.Draw();
            #endif

            profiler.EndSection("draw");
            profiler.EndFrame();
        }

        public Vector3 CameraPosition
        {
            get { return currentCamera.GetVector3("position"); }
        }

        public Matrix View
        {
            get { return currentCamera.GetMatrix("view"); }
        }

        public Matrix Projection
        {
            get { return currentCamera.GetMatrix("projection"); }
        }

        public Vector3 EyePosition
        {
            get
            {
                return currentCamera.GetVector3("position");
            }
        }

        public float EffectsVolume
        {
            get { return effectsVolume; }
            set { effectsVolume = value; }
        }

        public float MusicVolume
        {
            get { return musicVolume; }
            set { musicVolume = value; }
        }

        public List<RobotInfo> Robots
        {
            get { return robots; }
        }

        public List<LevelInfo> Levels
        {
            get { return levels; }
        }

        #if !XBOX
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

        public ProjectMagma.Profiler.Profiler Profiler
        {
            get { return profiler; }
        }
    }

    public delegate void IntervalExecutionAction(int times);
    public delegate void PushBackFinishedHandler();
}
