using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#if !XBOX
using xWinFormsLib;
#endif

using ProjectMagma.Framework;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Collision;
using System.Collections.Generic;

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

        // game paused?
        private bool paused = false;

        private HUD hud;
        private Menu menu;
        private Entity currentCamera;
        private float effectsVolume = 0;
        private float musicVolume = 0;

        private EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private EntityKindManager iceSpikeManager;
        private CollisionManager collisionManager;

        private GameTime currentGameTime;
        private double lastUpdateAt = 0;

        #region shadow related stuff
        // see http://www.ziggyware.com/readarticle.php?article_id=161

        // HACK: shouldnt be public, maybe extract this to some global rendering stuff class?
        public Matrix lightView;
        public Matrix lightProjection;
        public Vector3 lightPosition = new Vector3(0, 10000, 0); // later: replace by orthographic light, not lookAt
        public Vector3 lightTarget = Vector3.Zero;
        public Texture2D lightResolve;
        public Effect shadowEffect;
        int shadowMapSize = 1024;
        DepthStencilBuffer shadowStencilBuffer;
        RenderTarget2D lightRenderTarget;
        // ENDHACK

#if !XBOX
        private FormCollection formCollection;
        private ManagementForm managementForm;
#endif

        #endregion

        private static Game instance;

        public Effect testEffect; // public because it's only a test
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

            entityManager = new EntityManager();
            pillarManager = new EntityKindManager(entityManager, "pillar");
            islandManager = new EntityKindManager(entityManager, "island");
            playerManager = new EntityKindManager(entityManager, "player");
            powerupManager = new EntityKindManager(entityManager, "powerup");
            iceSpikeManager = new EntityKindManager(entityManager, "ice_spike");
            collisionManager = new CollisionManager();

            appliedAt = new Dictionary<String, double>(entityManager.Count);

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

        private void CreateManagementForm()
        {

            //Show the form
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

            // load default level
            LevelData levelData = Content.Load<LevelData>("Level/TestLevel");

            testEffect = Content.Load<Effect>("Effects/TestEffect");
            shadowEffect = Content.Load<Effect>("Effects/ShadowEffect");

            entityManager.Load(levelData);

            //Random islandRand = new Random(3432);
            //for(int i=0; i<10; ++i)
            //{
            //    Entity e = new Entity(entityManager, "dyn_island" + i);
            //    e.AddAttribute("kind", "string", "island");
            //    e.AddAttribute("velocity", "float3", "0 0 0");
            //    e.AddAttribute("acceleration", "float3", "0 0 0");
            //    e.AddAttribute("scale", "float3", "40 40 40");
            //    e.AddAttribute("mesh", "string", "Models/islandproto_v002");
            //    e.AddAttribute("bv_type", "string", "cylinder");
            //    e.AddAttribute("position", "float3",
            //        (int) (islandRand.NextDouble()*800-400) + " 100 " +
            //        (int) (islandRand.NextDouble()*800-400) );
            //    e.AddProperty("controller", new IslandControllerProperty());
            //    e.AddProperty("shadow_cast", new ShadowCastProperty());
            //    e.AddProperty("render", new RenderProperty());
            //    e.AddProperty("render_highlight", new RenderHighlightProperty());
                
            //    entityManager.AddDeferred(e);
            //}

            // set gamepad assignments
            int gi = 0;
            foreach (Entity e in playerManager)
            {
                e.AddIntAttribute("game_pad_index", gi++);
            }

            // load hud and menu
            hud.LoadContent();
            menu.LoadContent();

            // preload sounds
            foreach (Entity e in Game.Instance.powerupManager)
            {
                Game.Instance.Content.Load<SoundEffect>("Sounds/" + e.GetString("pickup_sound"));
            }
            Game.Instance.Content.Load<SoundEffect>("Sounds/gong1");
            Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/sword-clash");
            Game.Instance.Content.Load<SoundEffect>("Sounds/death");

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

             //lightProjection = Matrix.CreatePerspectiveFieldOfView(
                //MathHelper.ToRadians(10.0f), 1.0f, 1.0f, 10000.0f);
            lightProjection = Matrix.CreateOrthographic(1500, 1500,
                0.0f, 10000.0f);


            // Set the light to look at the center of the scene.
            lightView = Matrix.CreateLookAt(lightPosition,
                                            lightTarget,
                                            new Vector3(0, 0, -1));

            // later: replace by something like this:
            
            lightRenderTarget = new RenderTarget2D(graphics.GraphicsDevice,
                                 shadowMapSize,
                                 shadowMapSize,
                                 1,
                                 SurfaceFormat.Color);

            // Create out depth stencil buffer, using the shadow map size, 
            //and the same format as our regular depth stencil buffer.
            shadowStencilBuffer = new DepthStencilBuffer(
                        graphics.GraphicsDevice,
                        shadowMapSize,
                        shadowMapSize,
                        graphics.GraphicsDevice.DepthStencilBuffer.Format);
            currentCamera = entityManager["camera1"];

            CreateManagementForm();

            // play that funky musik white boy
            /*MediaPlayer.Play(Game.Instance.Content.Load<Song>("Sounds/music"));
            MediaPlayer.Volume = musicVolume;*/

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

            currentGameTime = gameTime;

            // fullscreen
            if(Keyboard.GetState().IsKeyDown(Keys.Enter)
                && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                graphics.IsFullScreen = !this.graphics.IsFullScreen;
                graphics.ApplyChanges();
            }

            if (!paused)
            {
                // update all entities
                foreach (Entity e in entityManager)
                {
                    e.OnUpdate(gameTime);
                }

                // perform collision detection
                collisionManager.Update(gameTime);

                // execute deferred add/remove orders on the entityManager
                entityManager.ExecuteDeferred();
            }

#if !XBOX
            // update the user interface
            formCollection.Update(gameTime);
#endif

            // update menu
            menu.Update(gameTime);

            // update all GameComponents registered
            base.Update(gameTime);

            // set lastupdate time
            lastUpdateAt = gameTime.TotalGameTime.TotalMilliseconds;
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

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // 1) Set the light's render target
            graphics.GraphicsDevice.SetRenderTarget(0, lightRenderTarget);

            // 2) Render the scene from the perspective of the light
            RenderShadow(gameTime);

            // 3) Set our render target back to the screen, and get the 
            //depth texture created by step 2
            graphics.GraphicsDevice.SetRenderTarget(0, null);
            lightResolve = lightRenderTarget.GetTexture();

            // 4) Render the scene from the view of the camera, 
            //and do depth comparisons in the shader to determine shadowing
            RenderScene(gameTime);

            // will apply effect such as bloom
            base.Draw(gameTime);

            // draw stuff which should net be filtered
            hud.Draw(gameTime);
            menu.Draw(gameTime);

#if !XBOX
            formCollection.Draw();
#endif
        }

        private void RenderScene(GameTime gameTime)
        {
            foreach (Entity e in entityManager)
            {
                e.OnDraw(gameTime, RenderMode.RenderToScene);
            }
            foreach (Entity e in entityManager)
            {
                e.OnDraw(gameTime, RenderMode.RenderToSceneAlpha);
            }
        }

        private void RenderShadow(GameTime gameTime)
        {
            // backup stencil buffer
            DepthStencilBuffer oldStencilBuffer
                = graphics.GraphicsDevice.DepthStencilBuffer;

            graphics.GraphicsDevice.DepthStencilBuffer = shadowStencilBuffer;
            graphics.GraphicsDevice.Clear(Color.White);

            foreach (Entity e in entityManager)
            {
                e.OnDraw(gameTime, RenderMode.RenderToShadowMap);
            }

            // restore stencil buffer
            graphics.GraphicsDevice.DepthStencilBuffer = oldStencilBuffer;
        }

        public GraphicsDeviceManager Graphics
        {
            get
            {
                return graphics;
            }
        }

        public EntityKindManager PillarManager
        {
            get
            {
                return pillarManager;
            }
        }

        public EntityKindManager IslandManager
        {
            get
            {
                return islandManager;
            }
        }

        public EntityKindManager PlayerManager
        {
            get
            {
                return playerManager;
            }
        }

        public EntityKindManager PowerupManager
        {
            get
            {
                return powerupManager;
            }
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

        public EntityManager EntityManager
        { 
            get
            {
                return entityManager;
            }
        }

        public CollisionManager CollisionManager
        {
            get
            {
                return collisionManager;
            }
        }

        public double LastUpdateAt
        {
            get { return lastUpdateAt; }
        }

        public GameTime CurrentUpdateTime
        {
            get { return currentGameTime; }
        }

        public float EffectsVolume
        {
            get { return effectsVolume; }
        }

        public float MusicVolume
        {
            get { return musicVolume; }
        }

        public bool Paused
        {
            get { return paused; }
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


        /**
         * TODO:
         * move stuff below into utility class
         **/

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
                float dt = Game.Instance.CurrentUpdateTime.ElapsedGameTime.Milliseconds / 1000.0f;

                Console.WriteLine("pushback applied");

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
            double current = currentGameTime.TotalGameTime.TotalMilliseconds;

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
    }

    public delegate void IntervalExecutionAction(int times);
}
