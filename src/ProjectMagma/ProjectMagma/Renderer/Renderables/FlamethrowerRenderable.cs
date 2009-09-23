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
    public class FlamethrowerRenderable : ParticleSystemRenderable
    {
        public FlamethrowerRenderable(
            double timestamp,
            int renderPriority,
            Vector3 position,
            Vector3 direction,
            bool fueled
        )
        :   base(timestamp, renderPriority, position)
        {
            this.direction = direction;
            this.fueled = fueled;

            flamethrowerEmitter = null;
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);
        }

        public override void UnloadResources(Renderer renderer)
        {
            base.UnloadResources(renderer);
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);

            if (fueled && flamethrowerEmitter == null)
            {
                flamethrowerEmitter = new FlamethrowerEmitter(Position, Direction, 2500);
                renderer.FlamethrowerSystem.AddEmitter(flamethrowerEmitter);
            }

            if (!fueled && flamethrowerEmitter != null)
            {
                renderer.FlamethrowerSystem.RemoveEmitter(flamethrowerEmitter);
                flamethrowerEmitter = null;
            }

            if (flamethrowerEmitter != null)
            {
                flamethrowerEmitter.Point = Position;
                flamethrowerEmitter.Direction = Direction;
            }
        }

        public override void Draw(Renderer renderer)
        {
            base.Draw(renderer);
        }

        public override void UpdateBool(string id, double timestamp, bool value)
        {
            base.UpdateBool(id, timestamp, value);

            if (id == "Fueled")
            {
                fueled = value;
            }
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "Direction")
            {
                direction = value;
            }
        }

        public Vector3 Direction
        {
            get { return direction; }
        }

        public bool Fueled
        {
            get { return fueled; }
        }

        private Vector3 direction;
        private bool fueled;
        private FlamethrowerEmitter flamethrowerEmitter;
    }
}
