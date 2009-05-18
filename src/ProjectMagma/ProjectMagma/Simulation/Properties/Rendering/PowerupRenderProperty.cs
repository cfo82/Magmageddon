using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class PowerupRenderProperty : BasicRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Entity entity, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new PowerupRenderable(Game.Instance.Simulation.Time.At, scale, rotation, position, model);
        }
    }
}
