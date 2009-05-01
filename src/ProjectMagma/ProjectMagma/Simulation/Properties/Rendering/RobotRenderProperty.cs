using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class RobotRenderProperty : BasicRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new RobotRenderable(scale, rotation, position, model);
        }
    }
}
