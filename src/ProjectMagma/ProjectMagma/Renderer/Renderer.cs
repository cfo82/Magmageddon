using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class Renderer
    {
        public Renderer(
            ContentManager content,
            GraphicsDevice device
        )
        {
            EnablePostProcessing = true;

            this.device = device;
            shadowCaster = new List<Renderable>();
            opaqueRenderables = new List<Renderable>();
            transparentRenderables = new List<Renderable>();
            overlays = new List<Renderable>();

            shadowEffect = content.Load<Effect>("Effects/ShadowEffect");

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

            vectorCloudTexture = Game.Instance.Content.Load<Texture2D>("Textures/Lava/vectorclouds");
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
        }
        
        public void Render(GameTime gameTime)
        {
            LightManager.Update(gameTime);

            device.Clear(Color.CornflowerBlue);

            // 1) Set the light's render target
            device.SetRenderTarget(0, lightRenderTarget);

            // 2) Render the scene from the perspective of the light
            RenderShadow(gameTime);

            // 3) Set our render target back to the screen, and get the 
            //depth texture created by step 2
            device.SetRenderTarget(0, null);
            lightResolve = lightRenderTarget.GetTexture();

            // 4) Render the scene from the view of the camera, 
            //and do depth comparisons in the shader to determine shadowing
            RenderScene(gameTime);

            // 5) Bloom (later HDR etc)
            foreach (GameComponent component in Game.Instance.Components)
            {
                DrawableGameComponent drawableComponent = component as DrawableGameComponent;
                if (drawableComponent != null)
                {
                    drawableComponent.Draw(gameTime);
                }
            }

            if (EnablePostProcessing)
            {
                RenderChannels = Target1.GetTexture();
                glowPass.GeometryRender = GeometryRender;
                glowPass.Render(gameTime);

                hdrCombinePass.GeometryRender = GeometryRender;

                hdrCombinePass.BlurGeometryRender = glowPass.BlurGeometryRender;
                hdrCombinePass.RenderChannelColor = glowPass.BlurRenderChannelColor;
                hdrCombinePass.Render(gameTime);
            }

            // 5) Render overlays
            RenderOverlays(gameTime);
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
            Vector3 cameraPosition = Game.Instance.CameraPosition;

            Vector3 r1Diff = cameraPosition - r1.Position;
            Vector3 r2Diff = cameraPosition - r2.Position;

            if (r1Diff.LengthSquared() > r2Diff.LengthSquared())
            {
                return -1;
            }
            else if (r1Diff.LengthSquared() < r2Diff.LengthSquared())
            {
                return 1;
            }
            else
            {
                return 0;
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
            transparentRenderables.Sort(TransparentRenderableComparison);

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

        public void AddRenderable(
            Renderable renderable
        )
        {
            List<Renderable> renderables = GetMatchingRenderableList(renderable);

            if (renderables.Contains(renderable))
            {
                throw new Exception("invalid addition of already registered renderable");
            }

            renderables.Add(renderable);
        }

        public void RemoveRenderable(
            Renderable renderable
        )
        {
            List<Renderable> renderables = GetMatchingRenderableList(renderable);
            
            if (!renderables.Contains(renderable))
            {
                throw new Exception("renderer does not contain the given renderable!");
            }

            renderables.Remove(renderable);
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

        public LightManager LightManager { get; set; }

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

        private RenderTarget2D Target0 { get; set; }
        private RenderTarget2D Target1 { get; set; }
        private RenderTarget2D Target2 { get; set; }

        public Texture2D GeometryRender { get; set; }
        public Texture2D RenderChannels { get; set; }

        public ResolveTexture2D ResolveTarget { get; set; }

        private GlowPass glowPass;
        private HdrCombinePass hdrCombinePass;

        //private LightManager lightManager;
    }
}
