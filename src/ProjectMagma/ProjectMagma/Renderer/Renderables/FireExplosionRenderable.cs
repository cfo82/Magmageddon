using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Model;
using ProjectMagma.Renderer.ParticleSystem.Emitter;
using ProjectMagma.Renderer.ParticleSystem.Stateful.Implementations;

namespace ProjectMagma.Renderer
{
    public class FireExplosionRenderable : PointExplosionRenderable
    {
        public FireExplosionRenderable(
            double timestamp, 
            Vector3 position
        )
        :   base(timestamp, position)
        {
        }

        protected override PointExplosion CreateExplosionSystem()
        {
            return new FireExplosion(Game.Instance.Renderer, Game.Instance.ContentManager, Game.Instance.GraphicsDevice);
        }

        protected override PointExplosionEmitter CreateExplosionEmitter(Vector3 position, double currentFrameTime)
        {
            return new FireExplosionEmitter(position, currentFrameTime);
        }
    }
}
