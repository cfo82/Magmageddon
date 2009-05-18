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
    public class FireExplosionRenderProperty : PointExplosionRenderProperty
    {
        protected override PointExplosionRenderable CreateExplosionRenderable(Entity entity, Vector3 position)
        {
            return new FireExplosionRenderable(Game.Instance.Simulation.Time.At, position);
        }
    }
}
