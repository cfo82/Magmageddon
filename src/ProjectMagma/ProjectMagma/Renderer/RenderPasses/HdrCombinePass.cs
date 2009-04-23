using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class HdrCombinePass : RenderPass
    {
        public HdrCombinePass(Renderer renderer, RenderTarget2D target0, RenderTarget2D target1)
            : base(renderer, target0, target1)
        {
            hdrCombineEffect = Game.Instance.Content.Load<Effect>("Effects/HdrCombine");
        }

        public override void Render(GameTime gameTime)
        {
            hdrCombineEffect.Parameters["GeometryRender"].SetValue(GeometryRender);
            hdrCombineEffect.Parameters["BlurGeometryRender"].SetValue(BlurGeometryRender);
            hdrCombineEffect.Parameters["RenderChannelColor"].SetValue(RenderChannelColor);
            DrawFullscreenQuad(Renderer.RenderChannels, hdrCombineEffect);
        }

        private Effect hdrCombineEffect;

        public Texture2D GeometryRender { get; set; }
        public Texture2D BlurGeometryRender { get; set; }
        public Texture2D RenderChannelColor { get; set; }
    }
}
