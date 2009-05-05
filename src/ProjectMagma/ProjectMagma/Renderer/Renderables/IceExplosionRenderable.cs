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
    public class IceExplosionRenderable : Renderable
    {
        public IceExplosionRenderable(
            Vector3 position
        )
        {
            this.position = position;

            lastFrameTime = currentFrameTime = 0.0;

            explosionEmitter = null;
        }

        public override void LoadResources()
        {
            base.LoadResources();

            explosionSystem = new IceExplosion(Game.Instance.Renderer, Game.Instance.ContentManager, Game.Instance.GraphicsDevice);
        }

        public override void UnloadResources()
        {
            explosionSystem.UnloadResources();

            base.UnloadResources();
        }

        public override void Update(Renderer renderer, GameTime gameTime)
        {
            // calculate the timestep to make
            lastFrameTime = currentFrameTime;
            double dtMs = (double)gameTime.ElapsedGameTime.Ticks / 10000d;
            double dt = dtMs / 1000.0;
            currentFrameTime = lastFrameTime + dt;

            if (explosionEmitter == null)
            {
                explosionEmitter = new IceExplosionEmitter(position, currentFrameTime);
                explosionSystem.AddEmitter(explosionEmitter);
            }

            explosionSystem.Update(lastFrameTime, currentFrameTime);
        }

        public override bool NeedsUpdate
        {
            get { return true; }
        }

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            explosionSystem.Render(Game.Instance.View, Game.Instance.Projection);
        }

        public override void UpdateVector3(string id, Vector3 value)
        {
            base.UpdateVector3(id, value);

            if (id == "Position")
            {
                position = value;
            }
        }

        public override Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public override RenderMode RenderMode
        {
            get { return RenderMode.RenderToSceneAlpha; }
        }

        private Vector3 position;

        private IceExplosionEmitter explosionEmitter;
        private IceExplosion explosionSystem;

        private double lastFrameTime;
        private double currentFrameTime;
    }
}
