using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    class DownscalePass : RenderPass
    {
        public DownscalePass(Renderer renderer)
        :   base(renderer)
        {
            downscaleEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Downscale");
        }

        public void Render(
            Texture2D hdrColorBuffer, Texture2D renderChannelBuffer,
            RenderTarget2D targetDownscaledHDRColorBuffer, RenderTarget2D targetDownscaledRenderChannelBuffer
            )
        {
            downscaleEffect.Parameters["HDRColorBuffer"].SetValue(hdrColorBuffer);
            downscaleEffect.Parameters["RenderChannelBuffer"].SetValue(renderChannelBuffer);
            downscaleEffect.Parameters["HalfPixelSize"].SetValue(new Vector2(
                1 / (2.0f * (float)targetDownscaledHDRColorBuffer.Width),
                1 / (2.0f * (float)targetDownscaledHDRColorBuffer.Height)
                ));
            DrawFullscreenQuad(
                hdrColorBuffer,
                targetDownscaledHDRColorBuffer, targetDownscaledRenderChannelBuffer,
                downscaleEffect
                );
        }

        private Effect downscaleEffect;
    }
}