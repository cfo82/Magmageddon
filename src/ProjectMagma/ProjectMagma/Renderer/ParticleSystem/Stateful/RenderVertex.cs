using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public struct RenderVertex
    {
        // stores the corner of the vertex (we've got now 4 vertices per particle)
        public Short2 corner;
        // stores the particles coordinate in the texture
        public Vector2 particleCoordinate;

        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, VertexElementFormat.Short2, VertexElementUsage.Position, 0),
            new VertexElement(4, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        };

        public const int SizeInBytes = 12;
    }
}
