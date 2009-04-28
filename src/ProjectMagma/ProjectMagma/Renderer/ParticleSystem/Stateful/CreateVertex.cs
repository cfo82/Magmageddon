using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public struct CreateVertex
    {
        public CreateVertex(
            Vector3 particlePosition,
            Vector3 particleVelocity,
            Vector2 particleCoordinate
            )
        {
            this.ParticlePosition = particlePosition;
            this.ParticleVelocity = particleVelocity;
            this.ParticleCoordinate = particleCoordinate;
        }

        public Vector3 ParticlePosition;
        public Vector3 ParticleVelocity;
        public Vector2 ParticleCoordinate;

        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3,
                                    VertexElementMethod.Default,
                                    VertexElementUsage.Position, 0),

            new VertexElement(0, 12, VertexElementFormat.Vector3,
                                    VertexElementMethod.Default,
                                    VertexElementUsage.Normal, 0),

            new VertexElement(0, 24, VertexElementFormat.Vector2,
                                    VertexElementMethod.Default,
                                    VertexElementUsage.TextureCoordinate, 0),

        };

        public const int SizeInBytes = 32;
    }
}
