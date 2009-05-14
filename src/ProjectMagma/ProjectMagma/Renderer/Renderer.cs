using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Renderer.Interface;
using ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations;

namespace ProjectMagma.Renderer
{
    public class Renderer : RendererInterface
    {
        public Renderer(
            WrappedContentManager wrappedContent,
            GraphicsDevice device
        )
        {
            EnablePostProcessing = true;

            renderTime = new RenderTime(Game.Instance.GlobalClock.ContinuousMilliseconds,
                Game.Instance.GlobalClock.PausableMilliseconds);

            this.device = device;
            updateRenderables = new List<Renderable>();
            shadowCaster = new List<Renderable>();
            opaqueRenderables = new List<Renderable>();
            transparentRenderables = new List<Renderable>();
            overlays = new List<Renderable>();

            shadowEffect = wrappedContent.Load<Effect>("Effects/ShadowEffect");

            Camera = new Camera(this);

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
                                 SurfaceFormat.Color);

            // Create out depth stencil buffer, using the shadow map size, 
            //and the same format as our regular depth stencil buffer.
            shadowStencilBuffer = new DepthStencilBuffer(
                        device,
                        shadowMapSize,
                        shadowMapSize,
                        device.DepthStencilBuffer.Format);

            vectorCloudTexture = wrappedContent.Load<Texture2D>("Textures/Lava/vectorclouds");
            LightManager = new LightManager();


            // set up render targets
            PresentationParameters pp = Device.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;
            SurfaceFormat format = pp.BackBufferFormat;

            if (EnablePostProcessing)
            {
                //ResolveTarget = new ResolveTexture2D(Device, width, height, 1, format);
                Target0 = new RenderTarget2D(Device, width, height, 1, format);
                Target1 = new RenderTarget2D(Device, width, height, 1, format);
                Target2 = new RenderTarget2D(Device, width, height, 1, format);
                glowPass = new GlowPass(this, Target2, Target1);
                hdrCombinePass = new HdrCombinePass(this, Target0, Target1);
            }

            statefulParticleResourceManager = new ProjectMagma.Renderer.ParticleSystem.Stateful.ResourceManager(wrappedContent, device);

            explosionSystem = new LavaExplosion(this, Game.Instance.ContentManager, device);
            for (int i = 0; i < 50; ++i)
            {
                explosionSystem.AddEmitter(new ProjectMagma.Renderer.ParticleSystem.Emitter.LavaExplosionEmitter());
            }
            snowSystem = new Snow(this, Game.Instance.ContentManager, device);
            snowSystem.AddEmitter(new ProjectMagma.Renderer.ParticleSystem.Emitter.SnowEmitter(12f));
            for (int i = 0; i < 1000; ++i)
            {
                snowSystem.Update(-20d/1000d + i * -20d/1000d, -20d/1000d + i * -20d/1000d + 20d/1000d);
            }

            updateQueues = new List<RendererUpdateQueue>();
        }

        public void AddUpdateQueue(RendererUpdateQueue updateQueue)
        {
            lock (updateQueues)
            {
                updateQueues.Add(updateQueue);
            }
        }

        private RendererUpdateQueue GetNextUpdateQueue()
        {
            lock (updateQueues)
            {
                if (updateQueues.Count == 0)
                    { return null; }
                
                RendererUpdateQueue q = updateQueues[0];
                updateQueues.RemoveAt(0);
                return q;
            }
        }

        private double lastFrameTime = 0;
        private double currentFrameTime = 0;

        private void Update(GameTime gameTime)
        {
            lastFrameTime = currentFrameTime;
            double dtMs = (double)gameTime.ElapsedGameTime.Ticks / 10000d;
            double dt = dtMs / 1000.0;
            currentFrameTime = lastFrameTime + dt;

            Camera.Update(gameTime);
            Camera.RecomputeFrame(opaqueRenderables);
            RendererUpdateQueue q = GetNextUpdateQueue();
            while (q != null)
            {
                for (int i = 0; i < q.Count; ++i)
                {
                    q[i].Apply();
                }

                q = GetNextUpdateQueue();
            }

            foreach (Renderable renderable in updateRenderables)
            {
                renderable.Update(this, gameTime);
            }

            if (explosionSystem != null)
            {
                explosionSystem.Update(lastFrameTime, currentFrameTime);
            }
            if (snowSystem != null)
            {
                snowSystem.Update(lastFrameTime, currentFrameTime);
            }
        }
        
        public void Render(GameTime gameTime)
        {
            renderTime.Update();

            Game.Instance.Profiler.BeginSection("beginning_stuff");
            Update(gameTime);
            LightManager.Update(gameTime);

            device.Clear(Color.CornflowerBlue);

            // 1) Set the light's render target
            device.SetRenderTarget(0, lightRenderTarget);
            Game.Instance.Profiler.EndSection("beginning_stuff");

            // 2) Render the scene from the perspective of the light
            Game.Instance.Profiler.BeginSection("renderer_shadow");
            RenderShadow(gameTime);
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
            RenderScene(gameTime);
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
                glowPass.Render(gameTime);
                Game.Instance.Profiler.EndSection("renderer_post_glow");

                hdrCombinePass.GeometryRender = GeometryRender;

                hdrCombinePass.BlurGeometryRender = glowPass.BlurGeometryRender;
                hdrCombinePass.RenderChannelColor = glowPass.BlurRenderChannelColor;
                Game.Instance.Profiler.BeginSection("renderer_post_hdr");
                hdrCombinePass.Render(gameTime);
                Game.Instance.Profiler.EndSection("renderer_post_hdr");
                Game.Instance.Profiler.EndSection("renderer_post");
            }

            // 5) Render overlays
            Game.Instance.Profiler.BeginSection("overlay");
            RenderOverlays(gameTime);
            Game.Instance.Profiler.EndSection("overlay");
        }

        private void RenderShadow(GameTime gameTime)
        {
            // backup stencil buffer
            DepthStencilBuffer oldStencilBuffer
                = device.DepthStencilBuffer;

            device.DepthStencilBuffer = shadowStencilBuffer;
            device.Clear(Color.White);

            foreach (Renderable renderable in shadowCaster)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToShadowMap);
                renderable.Draw(this, gameTime);
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

        private void RenderScene(GameTime gameTime)
        {
            //RenderTarget2D oldRenderTarget0 = (RenderTarget2D) Device.GetRenderTarget(0);
            //RenderTarget2D oldRenderTarget1 = (RenderTarget2D) Device.GetRenderTarget(1);
            //Device.SetRenderTarget(0, oldRenderTarget1);
            //Device.SetRenderTarget(1, oldRenderTarget0);
            if (EnablePostProcessing)
            {
                Device.SetRenderTarget(0, Target0);
                Device.SetRenderTarget(1, Target1);
            }

            Device.Clear(Color.Black);

            foreach (Renderable renderable in opaqueRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToScene);
                renderable.Draw(this, gameTime);
            }

            // need to sort transparent renderables by position and render them (back to front!!)
            // TODO: validate sorting... 
            //transparentRenderables.Sort(TransparentRenderableComparison);
            if (explosionSystem != null)
            {
                explosionSystem.Render(lastFrameTime, currentFrameTime, Camera.View, Camera.Projection);
            }
            if (snowSystem != null)
            {
                snowSystem.Render(lastFrameTime, currentFrameTime, Camera.View, Camera.Projection);
            }

            foreach (Renderable renderable in transparentRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlpha);
                renderable.Draw(this, gameTime);
            }

            if (EnablePostProcessing)
            {
                Device.SetRenderTarget(0, null);
                Device.SetRenderTarget(1, null);
                GeometryRender = Target0.GetTexture();
                RenderChannels = Target1.GetTexture();
            }
        }

        #region post-processing tests


        #endregion

        private void RenderOverlays(GameTime gameTime)
        {
            foreach (Renderable renderable in overlays)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderOverlays);
                renderable.Draw(this, gameTime);
            }
        }

        private List<Renderable> GetMatchingRenderableList(
            Renderable renderable
        )
        {
            switch (renderable.RenderMode)
            {
                case RenderMode.RenderToShadowMap: return shadowCaster;
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
            
            renderables.Add(renderable);
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
        private List<Renderable> shadowCaster;
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

        public Texture2D GeometryRender { get; set; }
        public Texture2D RenderChannels { get; set; }

        public ResolveTexture2D ResolveTarget { get; set; }

        private GlowPass glowPass;
        private HdrCombinePass hdrCombinePass;

        private ParticleSystem.Stateful.Implementations.LavaExplosion explosionSystem;
        private ParticleSystem.Stateful.Implementations.Snow snowSystem;
        private ParticleSystem.Stateful.ResourceManager statefulParticleResourceManager;
        
        //private LightManager lightManager;

        // ACCESS TO THIS LIST ONLY FOR SYNCHRONIZED THINGS!!
        private List<RendererUpdateQueue> updateQueues;

        private RenderTime renderTime;
    }
}
