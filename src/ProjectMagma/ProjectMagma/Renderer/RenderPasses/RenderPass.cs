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
        }

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
            RenderTarget2D renderTarget0,
            Effect effect
        )
        {
            Renderer.Device.SetRenderTarget(0, renderTarget0);
            DrawFullscreenQuad
            (
                texture,
                renderTarget0.Width,
                renderTarget0.Height,
                effect
            );
            Renderer.Device.SetRenderTarget(0, null);
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
            Effect spriteEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Sm3SpriteBatch");

            Viewport viewport = Renderer.Device.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            spriteEffect.Parameters["ViewportSize"].SetValue(viewportSize);

            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            spriteEffect.Begin();
            spriteEffect.CurrentTechnique.Passes[0].Begin();

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

            spriteEffect.CurrentTechnique.Passes[0].End();
            spriteEffect.End();
        }

        private SpriteBatch spriteBatch;
    }
}
