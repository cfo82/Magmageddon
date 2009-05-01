using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer.ParticleSystem.Stateful
{
    public class ResourceManager
    {
        public ResourceManager(
            ContentManager content,
            GraphicsDevice device
        )
        {
            this.device = device;

            // Create vertexbuffers. One for each particle map size
            renderingVertexBuffers = new VertexBuffer[(int)Size.SizeCount];
            for (int i = 0; i < (int)Size.SizeCount; ++i)
            {
                Vector2 positionHalfPixel = new Vector2(1.0f / (2.0f * SizeMap[i]), 1.0f / (2.0f * SizeMap[i]));
                RenderVertex[] vertices = new RenderVertex[SizeMap[i]*SizeMap[i]];
                for (int x = 0; x < SizeMap[i]; ++x)
                {
                    for (int y = 0; y < SizeMap[i]; ++y)
                    {
                        vertices[y * SizeMap[i] + x].particleCoordinate = new Vector2(
                            positionHalfPixel.X + 2 * x * positionHalfPixel.X,
                            positionHalfPixel.Y + 2 * y * positionHalfPixel.Y);
                    }
                }
                renderingVertexBuffers[i] = new VertexBuffer(device, RenderVertex.SizeInBytes * vertices.Length, BufferUsage.WriteOnly | BufferUsage.Points);
                renderingVertexBuffers[i].SetData<RenderVertex>(vertices);
            }

            // create the vertex declaration for rendering
            renderingVertexDeclaration = new VertexDeclaration(device, RenderVertex.VertexElements);

            // allocate the stateMapLists
            stateMapLists = new List<RenderTarget2D>[(int)Size.SizeCount];
            for (int i = 0; i < (int)Size.SizeCount; ++i)
            {
                stateMapLists[i] = new List<RenderTarget2D>();
            }
        }

        public VertexBuffer GetRenderingVertexBuffer(
            Size size
        )
        {
            return renderingVertexBuffers[(int)size];
        }

        public VertexDeclaration GetRenderingVertexDeclaration()
        {
            return renderingVertexDeclaration;
        }

        public RenderTarget2D AllocateStateTexture(
            Size size
        )
        {
            List<RenderTarget2D> list = GetStateMapList(size);
            if (list.Count == 0)
            {
                RenderTarget2D stateMap = new RenderTarget2D(device, SizeMap[(int)size], SizeMap[(int)size], 1, SurfaceFormat.HalfVector4);
                return stateMap;
            }
            else
            {
                RenderTarget2D stateMap = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return stateMap;
            }
        }

        public void FreeStateTexture(
            Size size,
            RenderTarget2D stateMap
        )
        {
            List<RenderTarget2D> list = GetStateMapList(size);
            list.Add(stateMap);
        }

        private List<RenderTarget2D> GetStateMapList(
            Size size
        )
        {
            return stateMapLists[(int)size];
        }

        private GraphicsDevice device;
        private VertexBuffer[] renderingVertexBuffers;
        private VertexDeclaration renderingVertexDeclaration;
        private List<RenderTarget2D>[] stateMapLists;
        private static readonly int[] SizeMap = new int[] { 16, 32, 48, 64, 96, 128 };

    }
}
