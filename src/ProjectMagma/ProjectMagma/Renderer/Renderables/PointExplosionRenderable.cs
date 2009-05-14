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
            Vector3 position
        )
        :   base(position)
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

        public override void Update(Renderer renderer, GameTime gameTime)
        {
            base.Update(renderer, gameTime);

            if (explosionEmitter == null)
            {
                explosionEmitter = CreateExplosionEmitter(Position, CurrentFrameTime);
                explosionSystem.AddEmitter(explosionEmitter);
            }

            explosionSystem.Update(LastFrameTime, CurrentFrameTime);
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            explosionSystem.Render(LastFrameTime, CurrentFrameTime, renderer.Camera.View, renderer.Camera.Projection);
        }

        private PointExplosionEmitter explosionEmitter;
        private PointExplosion explosionSystem;
    }
}
