using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class TestPass : RenderPass
    {
        public TestPass(Renderer renderer, RenderTarget2D target0, RenderTarget2D target1)
            : base(renderer, target0, target1)
        {
        }

        public override void Render(GameTime gameTime)
        {
            DrawFullscreenQuad(Renderer.RenderChannels, Renderer.Device.Viewport.Width, Renderer.Device.Viewport.Height / 2, null);
            DrawFullscreenQuad(Renderer.ResolveTarget, Renderer.Device.Viewport.Width/2, Renderer.Device.Viewport.Height, null);
        }        

    }
}
