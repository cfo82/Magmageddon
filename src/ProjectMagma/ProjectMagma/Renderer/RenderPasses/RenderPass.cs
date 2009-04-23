using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

        public abstract void Render(GameTime gameTime);

        protected Renderer Renderer { get; set; }

        protected void DrawFullscreenQuad(
            Texture2D texture,
            RenderTarget2D renderTarget,
            Effect effect
        )
        {
            Renderer.Device.SetRenderTarget(0, renderTarget);
            DrawFullscreenQuad
            (
                texture,
                renderTarget.Width,
                renderTarget.Height,
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
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        private SpriteBatch spriteBatch;
    }
}
