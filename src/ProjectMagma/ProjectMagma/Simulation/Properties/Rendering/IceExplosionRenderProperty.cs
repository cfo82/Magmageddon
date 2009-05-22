using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class IceExplosionRenderProperty : PointExplosionRenderProperty
    {
        protected override PointExplosionRenderable CreateExplosionRenderable(Entity entity, Vector3 position)
        {
            return new IceExplosionRenderable(Game.Instance.Simulation.Time.At, position);
        }
    }
}
