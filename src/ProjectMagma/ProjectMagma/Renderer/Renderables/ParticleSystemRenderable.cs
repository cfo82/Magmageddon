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
        }

        public override bool NeedsUpdate
        {
            get { return true; }
        }

        public override void Draw(Renderer renderer)
        {
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "Position")
            {
                position = value;
            }
        }

        public override Vector3 Position
        {
            get { return position; }
        }

        public override RenderMode RenderMode
        {
            get { return RenderMode.RenderToSceneAlpha; }
        }
        
        private Vector3 position;
    }
}
