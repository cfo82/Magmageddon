using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class Billboard
    {
        private struct Vertex
        {
            public Vertex(Vector3 position, Vector2 textureCoordinate)
            {
                this.Position = position;
                this.TextureCoordinate = textureCoordinate;
            }

            public Vector3 Position;
            public Vector2 TextureCoordinate;

            public static readonly VertexElement[] VertexElements =
            {
                new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            };

            public const int SizeInBytes = 20;
        }

        public Billboard(
            Renderer renderer,
            Vector3 position,
            float width,
            float height,
            Vector4 color
            )
        {
            this.renderer = renderer;
            this.position = position;
            this.width = width;
            this.height = height;
            this.color = color;

            Vertex[] vertices = new Vertex[] {
                new Vertex(position, new Vector2(0, 0)),
                new Vertex(position, new Vector2(1, 0)),
                new Vertex(position, new Vector2(1, 1)),
                new Vertex(position, new Vector2(0, 0)),
                new Vertex(position, new Vector2(1, 1)),
                new Vertex(position, new Vector2(0, 1))
            };
            vertexDeclaration = new VertexDeclaration(Vertex.SizeInBytes, Vertex.VertexElements);
            vertexBuffer = new VertexBuffer(renderer.Device, vertexDeclaration, Vertex.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<Vertex>(vertices);

            effect = Game.Instance.ContentManager.Load<Effect>("Effects/Sfx/Billboard").Clone();
            Texture = Game.Instance.ContentManager.Load<Texture2D>("Textures/xna_logo");
        }

        public void Reposition(
            Vector3 position,
            float width,
            float height,
            Vector4 color
        )
        {
            this.position = position;
            this.width = width;
            this.height = height;
            this.color = color;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["BillboardPosition"].SetValue(position);
            effect.Parameters["BillboardWidth"].SetValue(width);
            effect.Parameters["BillboardHeight"].SetValue(height);
            effect.Parameters["BillboardColor"].SetValue(color);
            effect.Parameters["BillboardTexture"].SetValue(Texture);

            renderer.Device.SetVertexBuffer(vertexBuffer, 0);
            //renderer.Device.VertexDeclaration = vertexDeclaration;

            effect.CurrentTechnique = effect.Techniques["Billboards"];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                renderer.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        public Texture2D Texture { set; get; }

        private Renderer renderer;
        private Vector3 position;
        private float width;
        private float height;
        private Vector4 color;
        private VertexBuffer vertexBuffer;
        private VertexDeclaration vertexDeclaration;
        private Effect effect;
    }
}
