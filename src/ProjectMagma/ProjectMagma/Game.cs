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

        private HUD hud;
        private Menu menu;
        private Entity currentCamera;
        private float effectsVolume = 0.2f;
        private float musicVolume = 0.1f;
        
        private double lastUpdateAt = 0;

        private Simulation.Simulation simulation;
        private Renderer.Renderer renderer;

#if !XBOX
        private FormCollection formCollection;
        private ManagementForm managementForm;
#endif

        private static Game instance;

        BloomComponent bloom;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            hud = HUD.Instance;
            menu = Menu.Instance;

            // TODO: remove v-sync in future!?
            this.IsFixedTimeStep = false;
//            graphics.SynchronizeWithVerticalRetrace = false;
//            graphics.ApplyChanges();

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            Window.Title = "Project Magma";
            Content.RootDirectory = "Content";

            appliedAt = new Dictionary<String, double>();

            bloom = new BloomComponent(this);
            Components.Add(bloom);
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
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
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

            // TODO: move this
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

            // start simulation
            simulation = new ProjectMagma.Simulation.Simulation();
            simulation.Initialize(Content, "Level/TestLevel", new Entity[] {player1, player2});

            #if !XBOX
            managementForm.BuildForm();
            #endif

            // load hud and menu
            hud.LoadContent();
            menu.LoadContent();

            // preload sounds
            Game.Instance.Content.Load<SoundEffect>("Sounds/gong1");
            Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/sword-clash");
            Game.Instance.Content.Load<SoundEffect>("Sounds/death");

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

            currentCamera = simulation.EntityManager["camera1"];

            // play that funky musik white boy
            MediaPlayer.Play(Game.Instance.Content.Load<Song>("Sounds/music"));
            MediaPlayer.Volume = musicVolume;

            MediaPlayer.IsMuted = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
#if !XBOX
            formCollection.Dispose();
#endif
            MediaPlayer.Stop();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // don't do anything if not active app
            if (!IsActive)
            {
                base.Update(gameTime);
                return;
            }

            // fullscreen
            if(Keyboard.GetState().IsKeyDown(Keys.Enter)
                && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                graphics.IsFullScreen = !this.graphics.IsFullScreen;
                graphics.ApplyChanges();
            }

#if !XBOX
            // update the user interface
            formCollection.Update(gameTime);
#endif

            simulation.Update(gameTime);

            // update menu
            menu.Update(gameTime);

            // update all GameComponents registered
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if !XBOX
            formCollection.Render();
#endif

            renderer.Render(gameTime);

            // will apply effect such as bloom
            base.Draw(gameTime);

            // draw stuff which should net be filtered
            hud.Draw(gameTime);
            menu.Draw(gameTime);

#if !XBOX
            formCollection.Draw();
#endif
        }

        public Matrix View
        {
            get
            {
                return currentCamera.GetMatrix("view");
            }
        }

        public Matrix Projection
        {
            get
            {
                return currentCamera.GetMatrix("projection");
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


        /**
         * TODO:
         * move stuff below into utility class
         **/


        #region Strange Stuff used by Janicks code

        public static Vector3 GetPosition(Entity player)
        {
            return player.GetVector3("position");
        }

        public static Vector3 GetScale(Entity player)
        {
            if (player.HasVector3("scale"))
            {
                return player.GetVector3("scale");
            }
            else
            {
                return Vector3.One;
            }
        }

        public static Quaternion GetRotation(Entity player)
        {
            if (player.HasQuaternion("rotation"))
            {
                return player.GetQuaternion("rotation");
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        public static void ApplyPushback(ref Vector3 playerPosition, ref Vector3 pushbackVelocity, float deacceleration)
        {
            if (pushbackVelocity.Length() > 0)
            {
                float dt = Game.Instance.Simulation.CurrentGameTime.ElapsedGameTime.Milliseconds / 1000.0f;

//                Console.WriteLine("pushback applied: "+pushbackVelocity);

                Vector3 oldVelocity = pushbackVelocity;

                // apply de-acceleration
                pushbackVelocity -= Vector3.Normalize(pushbackVelocity) * deacceleration * dt;

                // if length increases we accelerate in opposite direction -> stop
                if (pushbackVelocity.Length() > oldVelocity.Length())
                    pushbackVelocity = Vector3.Zero;

                // apply velocity
                playerPosition += pushbackVelocity * dt;
            }
        }

        private readonly Dictionary<string, double> appliedAt;

        public void ApplyPerSecondAddition(Entity source, String identifier, int perSecond, ref int value)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalAddition(source, identifier, interval, ref value);
        }

        public void ApplyPerSecondAddition(Entity source, String identifier, int perSecond, IntAttribute attr)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalAddition(source, identifier, interval, attr);
        }

        public void ApplyPerSecondSubstractrion(Entity source, String identifier, int perSecond, ref int value)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalSubstraction(source, identifier, interval, ref value);
        }

        public void ApplyPerSecondSubstraction(Entity source, String identifier, int perSecond, IntAttribute attr)
        {
            float interval = 1000f / perSecond;
            ApplyIntervalSubstraction(source, identifier, interval, attr);
        }


        public void ApplyIntervalAddition(Entity source, String identifier, float interval, IntAttribute attr)
        {
            int val = attr.Value;
            ApplyIntervalAddition(source, identifier, interval, ref val);
            attr.Value = val;
        }

        public void ApplyIntervalAddition(Entity source, String identifier, float interval, ref int value)
        {
            int val = value;
            ExecuteAtInterval(source, identifier, interval, delegate(int diff) { val += diff; });
            value = val;
        }

        public void ApplyIntervalSubstraction(Entity source, String identifier, float interval, IntAttribute attr)
        {
            int val = attr.Value;
            ApplyIntervalSubstraction(source, identifier, interval, ref val);
            attr.Value = val;
        }

        public void ApplyIntervalSubstraction(Entity source, String identifier, float interval, ref int value)
        {
            int val = value;
            ExecuteAtInterval(source, identifier, interval, delegate(int diff) { val -= diff; });
            value = val;
        }

        public void ExecuteAtInterval(Entity source, String identifier, float interval, IntervalExecutionAction action)
        {
            String fullIdentifier = source.Name + "_" + identifier;
            double current = simulation.CurrentGameTime.TotalGameTime.TotalMilliseconds;

            // if appliedAt doesn't contain string this is first time we called this functin
            if (!appliedAt.ContainsKey(fullIdentifier))
            {
                appliedAt.Add(fullIdentifier, current);
                return;
            }

            double last = appliedAt[fullIdentifier];
            double nextUpdateTime = last + interval;
            // if we didnt adapt on last update, then there was no call to this method at that time
            // so we reset our time to current
            if (lastUpdateAt >= nextUpdateTime)
            {
                appliedAt[fullIdentifier] = current;
                return;
            }

            // do we have to update yet?
            if (current >= nextUpdateTime)
            {
                // calculate how many updates would have happened in between
                int times = (int)((current - last) / interval);
                
                // execute action
                action(times);
                
                // update time
                appliedAt[fullIdentifier] = last + times * interval;
            }
        }

        #endregion
    }

    public delegate void IntervalExecutionAction(int times);
}
