using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    class CombineDepthPass : RenderPass
    {
        public CombineDepthPass(Renderer renderer)
        :   base(renderer)
        {
            combineDepthEffect = Game.Instance.ContentManager.Load<Effect>("Effects/CombineDepth");
        }

        public void Render(
            Texture2D opaqueDepthBuffer, Texture2D transparentDepthBuffer,
            RenderTarget2D depthBuffer
            )
        {
            combineDepthEffect.Parameters["OpaqueDepthBuffer"].SetValue(opaqueDepthBuffer);
            combineDepthEffect.Parameters["TransparentDepthBuffer"].SetValue(transparentDepthBuffer);
            DrawFullscreenQuad(
                opaqueDepthBuffer,
                depthBuffer,
                combineDepthEffect
                );
        }

        private Effect combineDepthEffect;
    }
}