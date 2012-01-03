using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer.ParticleSystem
{
    public struct CreateVertex
    {
        public CreateVertex(
            Vector3 particlePosition,
            Vector3 particleVelocity,
            Vector2 particleCoordinate,
            Short2 targetTextureCoordinate,
            float emitterIndex
            )
        {
            this.ParticlePosition = particlePosition;
            this.TargetTextureCoordinate = targetTextureCoordinate;
            this.ParticleVelocity = particleVelocity;
            this.ParticleCoordinate = particleCoordinate;
            this.EmitterIndex = emitterIndex;
        }

        public Vector3 ParticlePosition;
        public Vector3 ParticleVelocity;
        public Vector2 ParticleCoordinate;
        public Short2 TargetTextureCoordinate;
        public float EmitterIndex;

        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Short2, VertexElementUsage.Position, 1),
            new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
        };

        public const int SizeInBytes = 40;
    }
}
