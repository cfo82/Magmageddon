using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public abstract class RenderPass
    {
        public RenderPass(Renderer renderer)
        {
            this.Renderer = renderer;

            this.spriteBatch = new SpriteBatch(renderer.Device);

            testSamplerState = new SamplerState();
            testSamplerState.Filter = TextureFilter.Point;
            testSamplerState.AddressU = TextureAddressMode.Clamp;
            testSamplerState.AddressV = TextureAddressMode.Clamp;
            testSamplerState.AddressW = TextureAddressMode.Clamp;
        }

        protected Renderer Renderer { get; set; }

        protected void DrawFullscreenQuad(
            Texture2D texture,
            RenderTarget2D renderTarget0,
            RenderTarget2D renderTarget1,
            Effect effect
        )
        {
            Renderer.Device.SetRenderTargets(renderTarget0, renderTarget1);
            DrawFullscreenQuad
            (
                texture,
                renderTarget0.Width,
                renderTarget0.Height,
                effect
            );
            Renderer.Device.SetRenderTarget(null);
        }

        protected void DrawFullscreenQuad(
            Texture2D texture,
            RenderTarget2D renderTarget0,
            Effect effect
        )
        {
            Renderer.Device.SetRenderTargets(renderTarget0);
            DrawFullscreenQuad
            (
                texture,
                renderTarget0.Width,
                renderTarget0.Height,
                effect
            );
            Renderer.Device.SetRenderTargets(null);
        }

        protected void DrawFullscreenQuad(
           Texture2D texture,
           Effect effect
        )
        {
            DrawFullscreenQuad(texture, Renderer.Device.Viewport.Width, Renderer.Device.Viewport.Height, effect);
        }

        protected void DrawFullscreenQuad(
            Texture2D texture,
            int width, int height,
            Effect effect
        )
        {
            //Effect spriteEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Sm3SpriteBatch");

            Viewport viewport = Renderer.Device.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            if (effect.Parameters["ViewportSize"] != null)
            {
                effect.Parameters["ViewportSize"].SetValue(viewportSize);
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null, effect);

            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);

            spriteBatch.End();
        }

        private SpriteBatch spriteBatch;
        private SamplerState testSamplerState;
    }
}
