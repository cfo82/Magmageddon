using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    class CombinePass : RenderPass
    {
        public CombinePass(Renderer renderer)
        :   base(renderer)
        {
            combineEffect = Game.Instance.ContentManager.Load<Effect>("Effects/Combine");
        }

        public void Render(
            Texture2D opaqueColorBuffer, Texture2D transparentColorBuffer,
            RenderTarget2D hdrColorBuffer
            )
        {
            combineEffect.Parameters["OpaqueColorBuffer"].SetValue(opaqueColorBuffer);
            combineEffect.Parameters["TransparentColorBuffer"].SetValue(transparentColorBuffer);
            DrawFullscreenQuad(
                opaqueColorBuffer,
                hdrColorBuffer,
                combineEffect
                );
        }

        private Effect combineEffect;
    }
}