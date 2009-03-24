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
using ProjectMagma.Shared.BoundingVolume;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Collision;
using System.Threading;

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

        private HUD hud;
        private Entity currentCamera;

        private EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private EntityKindManager iceSpikeManager;
        private CollisionManager collisionManager;

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

            // set gamepad assignments
            int gi = 0;
            foreach (Entity e in playerManager)
            {
                e.AddIntAttribute("game_pad_index", gi++);
            }

            // load hud
            hud.LoadContent();

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
            MediaPlayer.Volume = 0.3f;*/

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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // fullscreen
            if(Keyboard.GetState().IsKeyDown(Keys.Enter)
                && Keyboard.GetState().IsKeyDown(Keys.LeftAlt))
            {
                graphics.IsFullScreen = !this.graphics.IsFullScreen;
                graphics.ApplyChanges();
            }

            // update all entities
            foreach (Entity e in entityManager)
            {
                e.OnUpdate(gameTime);
            }
            // perform collision detection
            collisionManager.Update(gameTime);

            // execute deferred add/remove orders on the entityManager
            entityManager.ExecuteDeferred();

#if !XBOX
            // update the user interface
            formCollection.Update(gameTime);
#endif

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
         * TODO: move those into collision-manager
         *  and maybe GetPosition/rotation/scale into some utility class
         * HELPER functions, refactor!
           */

        public static BoundingSphere CalculateBoundingSphere(Entity entity)
        {
            Model mesh = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            Vector3 position = GetPosition(entity);
            Vector3 scale = GetScale(entity);
            Quaternion rotation = GetRotation(entity);

            // calculate center
            BoundingBox bb = CalculateBoundingBox(mesh, position, rotation, scale);
            Vector3 center = (bb.Min + bb.Max) / 2;

            // calculate radius
            //            float radius = (bb.Max-bb.Min).Length() / 2;
            float radius = (bb.Max.Y - bb.Min.Y) / 2; // HACK: hack for player

            return new BoundingSphere(center, radius);
        }

        // calculates y-axis aligned bounding cylinder
        public static BoundingCylinder CalculateBoundingCylinder(Entity entity)
        {
            Model mesh = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            Vector3 position = GetPosition(entity);
            Vector3 scale = GetScale(entity);
            Quaternion rotation = GetRotation(entity);

            // calculate center
            BoundingBox bb = CalculateBoundingBox(mesh, position, rotation, scale);
            Vector3 center = (bb.Min + bb.Max) / 2;

            float top = bb.Max.Y;
            float bottom = bb.Min.Y;

            // calculate radius
            // a valid cylinder here is an extruded circle (not an oval) therefore extents in 
            // x- and z-direction should be equal.
            float radius = bb.Max.X - center.X;

            return new BoundingCylinder(new Vector3(center.X, top, center.Z),
                new Vector3(center.X, bottom, center.Z),
                radius);
        }

        public static BoundingBox CalculateBoundingBox(Entity entity)
        {
            Model mesh = Game.Instance.Content.Load<Model>(entity.GetString("mesh"));
            Vector3 position = GetPosition(entity);
            Vector3 scale = GetScale(entity);
            Quaternion rotation = GetRotation(entity);

            return CalculateBoundingBox(mesh, position, rotation, scale);
        }

        public static BoundingBox CalculateBoundingBox(Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix world = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);

            BoundingBox bb = (BoundingBox)model.Tag;
            bb.Min = Vector3.Transform(bb.Min, world);
            bb.Max = Vector3.Transform(bb.Max, world);
            return bb;
        }

        public static float Pow2(float a)
        {
            return a * a;
        }

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

        public static double ApplyPerSecond(double current, double last, int perSecond, ref int value)
        {
            float interval = 1000f / perSecond;
            return ApplyInterval(current, last, interval, ref value);
        }

        public static double ApplyInterval(double current, double last, float interval, ref int value)
        {
            if (current >= last + interval)
            {
                int times = (int)((current - last) / interval);
                value -= times;
                return last + times * interval;
            }
            else
                return last;
        }
    }
}
