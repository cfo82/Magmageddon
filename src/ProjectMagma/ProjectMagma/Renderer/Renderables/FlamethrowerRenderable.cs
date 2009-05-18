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
            Vector3 position,
            Vector3 direction,
            bool fueled
        )
        :   base(timestamp, position)
        {
            this.direction = direction;
            this.fueled = fueled;

            flamethrowerEmitter = null;
            flamethrowerSystem = null;
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);

            flamethrowerSystem = new Flamethrower(Game.Instance.Renderer, Game.Instance.ContentManager, Game.Instance.GraphicsDevice);
        }

        public override void UnloadResources()
        {
            flamethrowerSystem.UnloadResources();

            base.UnloadResources();
        }

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);

            if (fueled && flamethrowerEmitter == null)
            {
                flamethrowerEmitter = new FlamethrowerEmitter(Position, Direction, 2500);
                flamethrowerSystem.AddEmitter(flamethrowerEmitter);
            }

            if (!fueled && flamethrowerEmitter != null)
            {
                flamethrowerSystem.RemoveEmitter(flamethrowerEmitter);
                flamethrowerEmitter = null;
            }

            if (flamethrowerEmitter != null)
            {
                flamethrowerEmitter.Point = Position;
                flamethrowerEmitter.Direction = Direction;
            }

            flamethrowerSystem.Update(renderer.Time.PausableLast / 1000d, renderer.Time.PausableAt / 1000d);
        }

        public override void Draw(Renderer renderer)
        {
            flamethrowerSystem.Render(renderer.Time.PausableLast / 1000d, renderer.Time.PausableAt / 1000d);
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
        private Flamethrower flamethrowerSystem;
    }
}
