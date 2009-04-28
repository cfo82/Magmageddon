using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.ParticleSystem
{
    public struct NewParticle
    {
        public NewParticle(Vector3 position, Vector3 velocity)
        {
            this.Position = position;
            this.Velocity = velocity;
        }

        public readonly Vector3 Position;
        public readonly Vector3 Velocity;
    }
}
