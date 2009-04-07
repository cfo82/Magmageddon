using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation
{
    public enum RenderMode
    {
        RenderToShadowMap,
        RenderToScene,
        RenderToSceneAlpha
    }

    public delegate void DrawHandler(Entity sender, SimulationTime gameTime, RenderMode renderMode);
}
