using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    class RestoreDepthBufferPass : RenderPass
    {
        public RestoreDepthBufferPass(Renderer renderer)
        :   base(renderer)
        {
            restoreEffect = Game.Instance.ContentManager.Load<Effect>("Effects/RestoreDepthBuffer");
        }

        public void Render(
            Texture2D depthBuffer
            )
        {
            restoreEffect.Parameters["DepthBuffer"].SetValue(depthBuffer);
            DrawFullscreenQuad(
                depthBuffer,
                restoreEffect
                );
        }

        private Effect restoreEffect;
    }
}