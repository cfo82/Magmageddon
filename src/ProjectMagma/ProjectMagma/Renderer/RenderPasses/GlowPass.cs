using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class GlowPass : RenderPass
    {
        public GlowPass(Renderer Renderer)
            : base(Renderer)
        { }

        public override void Render(GameTime gameTime)
        {
            DrawFullscreenQuad(Renderer.RenderChannels, Renderer.Device.Viewport.Width, Renderer.Device.Viewport.Height / 2, null);
            DrawFullscreenQuad(Renderer.ResolveTarget, Renderer.Device.Viewport.Width/2, Renderer.Device.Viewport.Height, null);
        }


        private void DrawFullscreenQuad(
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


        private void DrawFullscreenQuad(
            Texture2D texture,
            int width, int height,
            Effect effect
        )
        {
            SpriteBatch spriteBatch = new SpriteBatch(Renderer.Device);
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            //if (showBuffer >= currentBuffer)
            //{
            //    effect.Begin();
            //    effect.CurrentTechnique.Passes[0].Begin();
            //}
            //BasicEffect beffect = new BasicEffect(Device, null);
            //beffect.TextureEnabled = true;
            //beffect.Texture = texture;
            //beffect.Begin();

            // Draw the quad.
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();

            //beffect.End();

            //// End the custom effect.
            //if (showBuffer >= currentBuffer)
            //{
            //    effect.CurrentTechnique.Passes[0].End();
            //    effect.End();
            //}
        }

    }
}
