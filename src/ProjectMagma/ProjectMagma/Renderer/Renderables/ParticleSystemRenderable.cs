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
    public class ParticleSystemRenderable : Renderable
    {
        public ParticleSystemRenderable(
            Vector3 position
        )
        {
            this.position = position;
            lastFrameTime = currentFrameTime = 0.0;
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);
        }

        public override void UnloadResources()
        {
            base.UnloadResources();
        }

        public override void Update(Renderer renderer)
        {
            lastFrameTime = renderer.Time.Last / 1000d;
            currentFrameTime = renderer.Time.At / 1000d;
        }

        public override bool NeedsUpdate
        {
            get { return true; }
        }

        public override void Draw(Renderer renderer)
        {
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

        protected double LastFrameTime
        {
            get { return lastFrameTime; }
        }

        protected double CurrentFrameTime
        {
            get { return currentFrameTime; }
        }
        
        private Vector3 position;
        protected double lastFrameTime;
        protected double currentFrameTime;
    }
}
