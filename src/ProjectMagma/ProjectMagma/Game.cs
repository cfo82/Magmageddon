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
using xWinFormsLib;

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
        
        // TODO:
        // HACK: shouldnt be public, maybe extract this to some global rendering stuff class?
        public Vector3 cameraPosition = new Vector3(0, 850, 1400);
        public Vector3 cameraTarget = new Vector3(0, 150, 0);
        public Matrix cameraView;
        public Matrix cameraProjection;
        // ENDHACK


        private EntityManager entityManager;
        private EntityKindManager pillarManager;
        private EntityKindManager islandManager;
        private EntityKindManager playerManager;
        private EntityKindManager powerupManager;
        private EntityKindManager iceSpikeManager;

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

        private FormCollection formCollection;
        private ManagementForm managementForm;


        #endregion

        private static Game instance;

        public Effect testEffect; // public because it's only a test
        private SpriteFont HUDFont;
        BloomComponent bloom;

        private Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Window.Title = "Project Magma";
            Content.RootDirectory = "Content";

            entityManager = new EntityManager();
            pillarManager = new EntityKindManager(entityManager, "pillar");
            islandManager = new EntityKindManager(entityManager, "island");
            playerManager = new EntityKindManager(entityManager, "player");
            powerupManager = new EntityKindManager(entityManager, "powerup");
            iceSpikeManager = new EntityKindManager(entityManager, "ice_spike");

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
            // create the gui system
            formCollection = new FormCollection(this.Window, Services, ref graphics);
            managementForm = new ManagementForm(formCollection);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            HUDFont = Content.Load<SpriteFont>("HUDFont");

            LevelData levelData = Content.Load<LevelData>("Level/TestLevel");

            testEffect = Content.Load<Effect>("Effects/TestEffect");
            shadowEffect = Content.Load<Effect>("Effects/ShadowEffect");

            foreach (EntityData entityData in levelData.entities)
            {
                entityManager.Add(entityData);
            }

            int gi = 0;
            foreach (Entity e in playerManager)
            {
                e.AddIntAttribute("game_pad_index", gi++);
            }

            // preload sounds
            foreach (Entity e in Game.Instance.powerupManager)
            {
                Game.Instance.Content.Load<SoundEffect>("Sounds/" + e.GetString("pickup_sound"));
            }
            Game.Instance.Content.Load<SoundEffect>("Sounds/punch2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/hit2");
            Game.Instance.Content.Load<SoundEffect>("Sounds/sword-clash");

            Viewport viewport = graphics.GraphicsDevice.Viewport;

            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

 
            cameraView = Matrix.CreateLookAt(cameraPosition,
                                       cameraTarget, Vector3.Up);


            cameraProjection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(22.5f),
                aspectRatio, 1.0f,
                10000.0f);

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


            CreateManagementForm();

            // play that funky musik white boy
            MediaPlayer.Play(Game.Instance.Content.Load<Song>("Sounds/music"));
            MediaPlayer.Volume = 0.1f;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            formCollection.Dispose();
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

            foreach (Entity e in entityManager)
            {
                e.OnUpdate(gameTime);
            }
            entityManager.ExecuteDeferred();

            formCollection.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            formCollection.Render();

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

            base.Draw(gameTime);

            DrawHud(gameTime);

            formCollection.Draw();
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

        private void DrawHud(GameTime gameTime)
        {
            // draw infos about state
            spriteBatch.Begin();
            int pos = 5;
            foreach (Entity e in playerManager)
            {
                Color color = Color.White;
                if (e.Name.Equals("player1"))
                    color = Color.LightGreen;
                else
                    color = Color.Yellow;
                if (e.GetInt("health") <= 0)
                    color = Color.Red;
                spriteBatch.DrawString(HUDFont, e.Name + "; health: " + e.GetInt("health") + ", energy: " + e.GetInt("energy") + ", fuel: " + e.GetInt("fuel")
                    + ", frozenTime: " + e.GetInt("frozen")+ "ms; pos: " + e.GetVector3("position").ToString(),
                    new Vector2(5, pos), color);
                pos += 20;
            }
            spriteBatch.DrawString(HUDFont, (1000f / gameTime.ElapsedGameTime.Milliseconds) + " fps", new Vector2(5, pos), Color.White);
            spriteBatch.End();
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
                return cameraView;
            }
        }

        public Matrix Projection
        {
            get
            {
                return cameraProjection;
            }
        }

        public EntityManager EntityManager
        {
            get
            {
                return entityManager;
            }
        }

        /**
           * HELPER functions, refactor!
           */

        public static BoundingSphere calculateBoundingSphere(Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // calculate center
            BoundingBox bb = calculateBoundingBox(model, position, rotation, scale);
            Vector3 center = (bb.Min + bb.Max) / 2;

            // calculate radius
            //            float radius = (bb.Max-bb.Min).Length() / 2;
            float radius = (bb.Max.Y - bb.Min.Y) / 2; // hack for player

            return new BoundingSphere(center, radius);
        }

        // calculates y-axis aligned bounding cylinder
        public static BoundingCylinder calculateBoundingCylinder(Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // calculate center
            BoundingBox bb = calculateBoundingBox(model, position, rotation, scale);
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

        public static BoundingBox calculateBoundingBox(Model model, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix world = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);

            BoundingBox bb = (BoundingBox)model.Tag;
            bb.Min = Vector3.Transform(bb.Min, world);
            bb.Max = Vector3.Transform(bb.Max, world);
            return bb;
        }

        public static float pow2(float a)
        {
            return a * a;
        }

    }

     public struct BoundingCylinder
    {
        private Vector3 c1;
        private Vector3 c2;
        private float radius;

        public BoundingCylinder(Vector3 c1, Vector3 c2, float radius)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.radius = radius;
        }

        public bool Intersects(BoundingSphere bs)
        {
            // check collision on y axis
            if (bs.Center.Y - bs.Radius < c1.Y && bs.Center.Y + bs.Radius > c2.Y)
            {
                // check collision in xz
                if (Game.pow2(bs.Center.X - c1.X) + Game.pow2(bs.Center.Z - c1.Z) < Game.pow2(bs.Radius + radius))
                    return true; 
            }

            return false;
        }

        public Vector3 Top
        {
            get { return c1; }
        }

        public Vector3 Bottom
        {
            get { return c2; }
        }

        public float Radius
        {
            get { return radius; }
        }

    }

      
}
