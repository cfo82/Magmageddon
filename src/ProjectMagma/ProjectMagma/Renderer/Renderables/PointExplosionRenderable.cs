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
    public abstract class PointExplosionRenderable : ParticleSystemRenderable
    {
        public PointExplosionRenderable(
            double timestamp,
            int renderPriority,
            Vector3 position
        )
        :   base(timestamp, renderPriority, position)
        {
            explosionEmitter = null;
        }

        protected abstract PointExplosion CreateExplosionSystem();
        protected abstract PointExplosionEmitter CreateExplosionEmitter(Vector3 position, double currentFrameTime);

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            explosionSystem = CreateExplosionSystem();
        }

        public override void UnloadResources()
        {
            explosionSystem.UnloadResources();

            base.UnloadResources();
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);

            if (explosionEmitter == null)
            {
                explosionEmitter = CreateExplosionEmitter(Position, renderer.Time.PausableAt / 1000d);
                explosionSystem.AddEmitter(explosionEmitter);
            }

            explosionSystem.Update(renderer.Time.PausableLast / 1000d, renderer.Time.PausableAt / 1000d);
        }

        public override void Draw(Renderer renderer)
        {
            explosionSystem.Render(renderer.Time.PausableLast / 1000d, renderer.Time.PausableAt / 1000d);
        }

        private PointExplosionEmitter explosionEmitter;
        private PointExplosion explosionSystem;
    }
}
