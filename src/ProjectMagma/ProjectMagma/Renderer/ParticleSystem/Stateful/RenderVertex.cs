using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public struct RenderVertex
    {
        public Vector2 particleCoordinate;

        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector2,
                                    VertexElementMethod.Default,
                                    VertexElementUsage.TextureCoordinate, 0)
        };

        public const int SizeInBytes = 8;
    }
}
