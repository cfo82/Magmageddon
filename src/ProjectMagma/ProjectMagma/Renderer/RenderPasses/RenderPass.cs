using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public abstract class RenderPass
    {
        public RenderPass(Renderer renderer, RenderTarget2D target0, RenderTarget2D target1)
        {
            this.Renderer = renderer;
            this.Target0 = target0;
            this.Target1 = target1;
            this.spriteBatch = new SpriteBatch(renderer.Device);
        }

        public abstract void Render();

        protected Renderer Renderer { get; set; }

        protected void DrawFullscreenQuad(
            Texture2D texture,
            RenderTarget2D renderTarget0,
            RenderTarget2D renderTarget1,
            Effect effect
        )
        {
            Renderer.Device.SetRenderTarget(0, renderTarget0);
            Renderer.Device.SetRenderTarget(1, renderTarget1);
            DrawFullscreenQuad
            (
                texture,
                renderTarget0.Width,
                renderTarget0.Height,
                effect
            );
            Renderer.Device.SetRenderTarget(0, null);
            Renderer.Device.SetRenderTarget(1, null);
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
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            if (effect != null)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
            if (effect != null)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        private SpriteBatch spriteBatch;
        protected RenderTarget2D Target0 { get; set; }
        protected RenderTarget2D Target1 { get; set; }
    }
}
