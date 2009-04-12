using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }
        
        public void Render(GameTime gameTime)
        {
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

        private void RenderScene(GameTime gameTime)
        {
            foreach (Renderable renderable in opaqueRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToScene);
                renderable.Draw(this, gameTime);
            }
            
            // TODO: need to sort transparent renderables by position (back to front!!)

            foreach (Renderable renderable in transparentRenderables)
            {
                Debug.Assert(renderable.RenderMode == RenderMode.RenderToSceneAlpha);
                renderable.Draw(this, gameTime);
            }
        }

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
            List<Renderable> renderables = null;
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
    }
}
