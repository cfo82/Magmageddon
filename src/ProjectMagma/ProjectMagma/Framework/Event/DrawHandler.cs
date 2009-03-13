using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Framework
{
    public enum RenderMode
    {
        RenderToShadowMap,
        RenderToScene
    }

    public delegate void DrawHandler(Entity sender, GameTime gameTime, RenderMode renderMode);
}
