using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Renderer.Interface;
using ProjectMagma.Renderer.ParticleSystem.Emitter;
using ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations;
using ProjectMagma.Renderer.Renderables;

namespace ProjectMagma.Renderer
{
    public class Renderer : RendererInterface
    {
        public enum RendererPhase
        {
            Intro,
            Game,
            Outro,
            Closed
        }

        public class ChangeLevelUpdate : RendererUpdate
        {
            public ChangeLevelUpdate(string levelName)
            {
                this.levelName = levelName;
            }

            public void Apply(double timestamp)
            {
                Game.Instance.Renderer.ChangeLevel(levelName);
            }

            private string levelName;
        }

        public class ChangeToPhaseUpdate : RendererUpdate
        {
            public ChangeToPhaseUpdate(RendererPhase newPhase, string winningPlayer, RendererUpdatable winningUpdatable)
            {
                this.newPhase = newPhase;
                this.winningPlayer = winningPlayer;
                this.winningUpdatable = winningUpdatable;
            }

            public void Apply(double timestamp)
            {
                Game.Instance.Renderer.ChangeToPhase(newPhase, winningPlayer, winningUpdatable);
            }

            private RendererPhase newPhase;
            private string winningPlayer;
            private RendererUpdatable winningUpdatable;
        }

        public Renderer(
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        {
            EnablePostProcessing = true;

            renderTime = new RenderTime(Game.Instance.GlobalClock.ContinuousMilliseconds,
                Game.Instance.GlobalClock.PausableMilliseconds);

            entityManager = new RendererEntityManager();

            this.device = device;
            updateRenderables = new List<Renderable>();
//            shadowCaster = new List<Renderable>();
            opaqueRenderables = new List<Renderable>();
            transparentRenderables = new List<Renderable>();
            overlays = new List<Renderable>();

            shadowEffect = wrappedContent.Load<Effect>("Effects/ShadowEffect");

            lightPosition = new Vector3(0, 10000, 0); // later: replace by orthographic light, not lookAt
            lightTarget = Vector3.Zero;
            lightProjection = Matrix.CreateOrthographic(1500, 1500, 0.0f, 10000.0f);

            // Set the light to look at the center of the scene.
            lightView = Matrix.CreateLookAt(
                lightPosition,
                lightTarget,
                new Vector3(0, 0, -1));

            // later: replace by something like this:

            lightRenderTarget = new RenderTarget2D(
                                 device,
                                 shadowMapSize,
                                 shadowMapSize,
                                 1,
                                 SurfaceFormat.Single);

            // Create out depth stencil buffer, using the shadow map size, 
            //and the same format as our regular depth stencil buffer.
            shadowStencilBuffer = new DepthStencilBuffer(
                        device,
                        shadowMapSize,
                        shadowMapSize,
                        device.DepthStencilBuffer.Format);

            vectorCloudTexture = wrappedContent.Load<Texture2D>("Textures/Lava/vectorclouds");

            // set up render targets
            PresentationParameters pp = Device.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            //SurfaceFormat format = SurfaceFormat.HalfVector4;
            //SurfaceFormat format = SurfaceFormat.Vector4;
            SurfaceFormat format = pp.BackBufferFormat;

            if (EnablePostProcessing)
            {
                //ResolveTarget = new ResolveTexture2D(Device, width, height, 1, format);
                Target0 = new RenderTarget2D(Device, width, height, 1, SurfaceFormat.HalfVector4);
                Target1 = new RenderTarget2D(Device, width, height, 1, format);
                Target2 = new RenderTarget2D(Device, width, height, 1, SurfaceFormat.HalfVector4);
                Target3 = new RenderTarget2D(Device, width, height, 1, format);
                DepthTarget = new RenderTarget2D(Device, width, height, 1, SurfaceFormat.Single);
                glowPass = new GlowPass(this, Target2, Target1);
                hdrCombinePass = new HdrCombinePass(this, Target0, Target1);
            }

            statefulParticleResourceManager = new ProjectMagma.Renderer.ParticleSystem.Stateful.ResourceManager(wrappedContent, device);

            updateQueues = new List<RendererUpdateQueue>();

            billboard = new Billboard(this, new Vector3(0, 200, 0), 250, 250, Vector4.One);
        }

        protected void ChangeLevel(
            string levelName
        )
        {
            EntityManager.Clear();
            EntityManager.Load(Game.Instance.ContentManager.Load<LevelData>(levelName));

            Camera = new Camera(this);

            LightManager = new LightManager(this);

            // recreate the lava fires system using the new level parameters
            if (explosionSystem != null)
                { explosionSystem.UnloadResources(); }
            explosionSystem = new LavaExplosion(this, Game.Instance.ContentManager, device,
                entityManager["lavafire"].GetFloat("size"),
                entityManager["lavafire"].GetFloat("rgb_multiplier"),
                entityManager["lavafire"].GetFloat("dot_multiplier"));
            for (int i = 0; i < 50; ++i)
            {
                explosionSystem.AddEmitter(new ProjectMagma.Renderer.ParticleSystem.Emitter.LavaExplosionEmitter());
            }

            // recreate the snow system using the new level parameters
            if (snowSystem != null)
                { snowSystem.UnloadResources(); }
            snowSystem = new Snow(this, Game.Instance.ContentManager, device,
                entityManager["snow"].GetFloat("particle_lifetime"),
                entityManager["snow"].GetFloat("max_alpha"),
                entityManager["snow"].GetFloat("base_size"),
                entityManager["snow"].GetFloat("random_size_modification"),
                entityManager["snow"].GetFloat("melting_start"),
                entityManager["snow"].GetFloat("melting_end"));
            snowSystem.AddEmitter(new SnowEmitter(EntityManager["snow"].GetFloat("particles_per_second")));
            for (int i = 0; i < 1000; ++i)
            {
                snowSystem.Update(-30d / 1000d + i * -30d / 1000d, -30d / 1000d + i * -30d / 1000d + 30d / 1000d);
            }
        }

        protected void ChangeToPhase(
            RendererPhase phase,
            string winningPlayer,
            RendererUpdatable winningUpdatable
        )
        {
            switch(phase)
            {
                case RendererPhase.Outro:
                    {
                        WinningScreenRenderable renderable = new WinningScreenRenderable(0, winningPlayer);
                        //RobotRenderable winningRobot = winningUpdatable as RobotRenderable;
                        //Debug.Assert(winningRobot != null);
                        //winningRobot.ActivatePermanentState("win");
                        renderable.LoadResources(this);
                        updateRenderables.Add(renderable);
                        overlays.Add(renderable);
                        break;
                    }
                case RendererPhase.Intro:
                    {
                        for (int i = 0; i < overlays.Count; ++i)
                        {
                            if (overlays[i] is WinningScreenRenderable)
                            {
                                overlays.RemoveAt(i);
                                break;
                            }
                        }
                        break;
                    }
            }
        }

        public void AddUpdateQueue(RendererUpdateQueue updateQueue)
        {
            lock (updateQueues)
            {
                updateQueues.Add(updateQueue);
            }
        }

        [Conditional("DEBUG")]
        private void ValicateUpdateQueueCount()
        {
            if (updateQueues.Count > 10000)
            {
                throw new System.Exception("error: renderer has more than 10000 update queues to process!");
            }
        }

        private RendererUpdateQueue GetNextUpdateQueue()
        {
            lock (updateQueues)
            {
                if (updateQueues.Count == 0)
                    { return null; }

                ValicateUpdateQueueCount();

                RendererUpdateQueue q = updateQueues[0];
                updateQueues.RemoveAt(0);
                return q;
            }
        }

        private double lastFrameTime = 0;
        private double currentFrameTime = 0;

        private void Update()
        {
            if(Camera!=null)
            {
                // should only be called if a level has already been loaded
                Camera.Update(this);
                Camera.RecomputeFrame(ref opaqueRenderables);
            }

            RendererUpdateQueue q = GetNextUpdateQueue();
            while (q != null)
            {
                for (int i = 0; i < q.Count; ++i)
                {
                    q[i].Apply(q.Timestamp);
                }

                q = GetNextUpdateQueue();
            }

            foreach (Renderable renderable in updateRenderables)
            {
                renderable.Update(this);
            }

            if (explosionSystem != null)
            {
                explosionSystem.Update(Time.Last / 1000d, Time.At / 1000d);
            }
            if (snowSystem != null)
            {
                snowSystem.Update(Time.Last/1000d, Time.At/1000d);
            }
        }
        
        public void Render()
        {
            renderTime.Update();
            //Console.WriteLine("rendering {0}", renderTime.PausableAt);

            Game.Instance.Profiler.BeginSection("beginning_stuff");
            Update();
            LightManager.Update(this);

            device.Clear(Color.White);

            // 1) Set the light's render target
            device.SetRenderTarget(0, lightRenderTarget);
            Game.Instance.Profiler.EndSection("beginning_stuff");

            // 2) Render the scene from the perspective of the light
            Game.Instance.Profiler.BeginSection("renderer_shadow");
            RenderShadow();
            Game.Instance.Profiler.EndSection("renderer_shadow");

            // 3) Set our render target back to the screen, and get the 
            //depth texture created by step 2
            Game.Instance.Profiler.BeginSection("change_render_target");
            device.SetRenderTarget(0, null);
            lightResolve = lightRenderTarget.GetTexture();
            Game.Instance.Profiler.EndSection("change_render_target");

            // 4) Render the scene from the view of the camera, 
            //and do depth comparisons in the shader to determine shadowing
            Game.Instance.Profiler.BeginSection("renderer_scene");
            RenderScene();
            Game.Instance.Profiler.EndSection("renderer_scene");

            // 5) Bloom (later HDR etc) -- commented out, we have no components anymore anyway
            //Game.Instance.Profiler.BeginSection("components");
            //foreach (GameComponent component in Game.Instance.Components)
            //{
            //    DrawableGameComponent drawableComponent = component as DrawableGameComponent;
            //    if (drawableComponent != null)
            //    {
            //        drawableComponent.Draw(gameTime);
            //    }
            //}
            //Game.Instance.Profiler.EndSection("components");

            if (EnablePostProcessing)
            {
                Game.Instance.Profiler.BeginSection("renderer_post");
                RenderChannels = Target1.GetTexture();
                glowPass.GeometryRender = GeometryRender;
                Game.Instance.Profiler.BeginSection("renderer_post_glow");
                glowPass.Render();
                Game.Instance.Profiler.EndSection("renderer_post_glow");

                hdrCombinePass.GeometryRender = GeometryRender;
                hdrCombinePass.BlurGeometryRender = glowPass.BlurGeometryRender;
                hdrCombinePass.RenderChannelColor = glowPass.BlurRenderChannelColor;
                hdrCombinePass.ToolTexture = ToolTexture;
                hdrCombinePass.DepthTexture = DepthMap;

                Game.Instance.Profiler.BeginSection("renderer_post_hdr");
                hdrCombinePass.Render();
                Game.Instance.Profiler.EndSection("renderer_post_hdr");
                Game.Instance.Profiler.EndSection("renderer_post");
            }

            RenderParticles();
            RenderSceneAfterPost();


            // 6) Render overlays
            Game.Instance.Profiler.BeginSection("overlay");
            RenderOverlays();
            Game.Instance.Profiler.EndSection("overlay");
        }

        private void RenderShadow()
        {
            //return;
            // backup stencil buffer
            DepthStencilBuffer oldStencilBuffer
                = device.DepthStencilBuffer;

            device.DepthStencilBuffer = shadowStencilBuffer;
            device.Clear(Color.Black);
            
            foreach (Renderable renderable in opaqueRenderables)
            {
                //Debug.Assert(renderable.RenderMode == RenderMode.RenderToShadowMap);
                Debug.Assert(renderable is ModelRenderable);
                if((renderable as ModelRenderable).IsShadowCaster)
                    (renderable as ModelRenderable).DrawToShadowMap(this);
            }

            // restore stencil buffer
            device.DepthStencilBuffer = oldStencilBuffer;

        }

        private int TransparentRenderableComparison(
            Renderable r1,
            Renderable r2
        )
        {
            Vector3 cameraPosition = Camera.Position;

            Vector3 r1Diff = cameraPosition - r1.Position;
            Vector3 r2Diff = cameraPosition - r2.Position;

            if (System.Math.Abs(r1Diff.LengthSquared() - r2Diff.LengthSquared()) < 1e-05)
            {
                return 0;
            }
            else if (r1Diff.LengthSquared() > r2Diff.LengthSquared())
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        private void RenderScene()
        {
            //RenderTarget2D oldRenderTarget0 = (RenderTarget2D) Device.GetRenderTarget(0);
            //RenderTarget2D oldRenderTarget1 = (RenderTarget2D) Device.GetRenderTarget(1);
            //Device.SetRenderTarget(0, oldRenderTarget1);
            //Device.SetRenderTarget(1, oldRenderTarget0);
            if (EnablePostProcessing)
            {
                Device.SetRenderTarget(0, Target0);
                Device.SetRenderTarget(1, Target1);
                Device.SetRenderTarget(2, Target3);
                Device.SetRenderTarget(3, DepthTarget);
            }

            Device.Clear(Color.White);

            foreach (Renderable renderable in opaqueRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToScene);
                renderable.Draw(this);
            }

            // need to sort transparent renderables by position and render them (back to front!!)
            // TODO: validate sorting... 

            if (EnablePostProcessing)
            {
                Device.SetRenderTarget(0, null);
                Device.SetRenderTarget(1, null);
                Device.SetRenderTarget(2, null);
                Device.SetRenderTarget(3, null);
                GeometryRender = Target0.GetTexture();
                RenderChannels = Target1.GetTexture();
                ToolTexture = Target3.GetTexture();
            }

            //Texture2D texture = DepthTarget.GetTexture();
            //float[] pixelData = new float[texture.Width * texture.Height];
            //texture.GetData(pixelData, 0, texture.Width * texture.Height);
            //Console.WriteLine("start");
            //for (int i = 0; i < texture.Width * texture.Height; i++)
            //    if (pixelData[i] != 0.0f)
            //    {
            //        float g = pixelData[i];
            //        Console.WriteLine(g);
            //    }
            //Console.WriteLine("end");
            //int a = 0;
        }

        private void RenderSceneAfterPost()
        {
            foreach (Renderable renderable in opaqueRenderables)
            {
                renderable.DrawAfterPost(this);
            }
        }

        private void RenderParticles()
        {
            //transparentRenderables.Sort(TransparentRenderableComparison);
            if (explosionSystem != null)
            {
                explosionSystem.Render(lastFrameTime, currentFrameTime);
            }
            if (snowSystem != null)
            {
                snowSystem.Render(lastFrameTime, currentFrameTime);
            }

            foreach (Renderable renderable in transparentRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlpha);
                renderable.Draw(this);
            }

        }

        private void RenderOverlays()
        {
            foreach (Renderable renderable in overlays)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderOverlays);
                renderable.Draw(this);
            }
        }

        private List<Renderable> GetMatchingRenderableList(
            Renderable renderable
        )
        {
            switch (renderable.RenderMode)
            {
                //case RenderMode.RenderToShadowMap: return shadowCaster;
                case RenderMode.RenderToScene: return opaqueRenderables;
                case RenderMode.RenderToSceneAlpha: return transparentRenderables;
                case RenderMode.RenderOverlays: return overlays;
                default: throw new Exception(string.Format("invalid RenderMode constant: {0}", renderable.RenderMode));
            }
        }

        //public void RecomputeLavaTemperature(Renderable lava, List<Renderable> lava)
        //{
        //    Texture2D tex = new Texture2D(device, 256, 256);

        //    lava;
        //}


        public void AddRenderable(
            Renderable renderable
        )
        {
            List<Renderable> renderables = GetMatchingRenderableList(renderable);

            if (
                renderables.Contains(renderable) ||
                (renderable.NeedsUpdate && updateRenderables.Contains(renderable))
                )
            {
                throw new Exception("invalid addition of already registered renderable");
            }

            renderable.LoadResources(this);

            if (renderable.NeedsUpdate)
            {
                updateRenderables.Add(renderable);
            }

            // insertion sort!
            int insertAtIndex = renderables.Count;
            for (int i = 0; i < renderables.Count; ++i)
            {
                if (
                    renderable.RenderPriority < renderables[i].RenderPriority &&
                    i < insertAtIndex
                    )
                {
                    insertAtIndex = i;
                    break;
                }
            }

            renderables.Insert(insertAtIndex, renderable);
        }

        public void RemoveRenderable(
            Renderable renderable
        )
        {
            List<Renderable> renderables = GetMatchingRenderableList(renderable);
            
            if (
                !renderables.Contains(renderable) ||
                (renderable.NeedsUpdate && !updateRenderables.Contains(renderable))
                )
            {
                throw new Exception("renderer does not contain the given renderable!");
            }

            renderable.UnloadResources();

            if (updateRenderables.Contains(renderable))
            {
                updateRenderables.Remove(renderable);
            }

            renderables.Remove(renderable);
        }

        public Vector3 CenterOfMass
        {
            get
            {
                Vector3 result = Vector3.Zero;
                int n = 0;
                foreach(Renderable renderable in updateRenderables)
                {
                    if(renderable is RobotRenderable)
                    {
                        result += renderable.Position;
                        n++;
                    }
                }
                return result / n;
            }
        }


        public GraphicsDevice Device
        {
            get { return device; }
        }

        public Texture2D LightResolve
        {
            get { return lightResolve; }
        }

        public RenderTarget2D LightRenderTarget
        {
            get { return lightRenderTarget; }
        }

        public DepthStencilBuffer ShadowStencilBuffer
        {
            get { return shadowStencilBuffer; }
        }

        public Effect ShadowEffect
        {
            get { return shadowEffect; }
        }

        public Vector3 LightPosition
        {
            get { return lightPosition; }
        }

        public Matrix LightView
        {
            get { return lightView; }
        }

        public Matrix LightProjection
        {
            get { return lightProjection; }
        }

        public ParticleSystem.Stateful.ResourceManager StatefulParticleResourceManager
        {
            get { return statefulParticleResourceManager; }
        }

        public RenderTime Time
        {
            get { return renderTime; }
        }

        public LightManager LightManager { get; set; }

        private List<Renderable> updateRenderables;
        //private List<Renderable> shadowCaster;
        private List<Renderable> opaqueRenderables;
        private List<Renderable> transparentRenderables;
        private List<Renderable> overlays;

        private GraphicsDevice device;
        private Matrix lightView;
        private Matrix lightProjection;
        private Vector3 lightPosition;
        private Vector3 lightTarget;
        private Texture2D lightResolve;
        private Effect shadowEffect;
        private int shadowMapSize = 1024;
        private DepthStencilBuffer shadowStencilBuffer;
        private RenderTarget2D lightRenderTarget;

        private Texture2D vectorCloudTexture;

        public bool EnablePostProcessing { get; set; }

        public Texture2D VectorCloudTexture
        {
            get { return vectorCloudTexture; }
        }

        public Camera Camera { get; set; }

        private RenderTarget2D Target0 { get; set; }
        private RenderTarget2D Target1 { get; set; }
        private RenderTarget2D Target2 { get; set; }
        private RenderTarget2D Target3 { get; set; }
        private RenderTarget2D DepthTarget { get; set; }

        public Texture2D DepthMap { get { return DepthTarget.GetTexture(); } }

        public Texture2D GeometryRender { get; set; }
        public Texture2D RenderChannels { get; set; }
        public Texture2D ToolTexture { get; set; }

        public ResolveTexture2D ResolveTarget { get; set; }

        public RendererEntityManager EntityManager
        {
            get { return entityManager; }
        }

        public Billboard Billboard
        {
            get { return billboard; }
        }
        
        private GlowPass glowPass;
        private HdrCombinePass hdrCombinePass;

        private ParticleSystem.Stateful.Implementations.LavaExplosion explosionSystem;
        private ParticleSystem.Stateful.Implementations.Snow snowSystem;
        private ParticleSystem.Stateful.ResourceManager statefulParticleResourceManager;
        
        //private LightManager lightManager;

        // ACCESS TO THIS LIST ONLY FOR SYNCHRONIZED THINGS!!
        private List<RendererUpdateQueue> updateQueues;

        private RenderTime renderTime;

        private RendererEntityManager entityManager;
        private Billboard billboard;
    }
}
