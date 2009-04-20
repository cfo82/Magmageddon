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
        public RenderPass()
        {
        }

        public abstract void Render(Renderer renderer, GameTime gameTime);
    }
}
