using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    public class RenderProperty : Property
    {
        public RenderProperty()
        {
            model = null;
        }

        public void OnAttached(Entity entity)
        {
            entity.Draw += new DrawHandler(OnDraw);

            // load the model
            string meshName = entity.GetString("mesh");
            model = Game.Instance.Content.Load<Model>(meshName);
        }

        public void OnDetached(Entity entity)
        {
            entity.Draw -= new DrawHandler(OnDraw);
        }

        private void OnDraw(Entity entity, GameTime gameTime)
        {
            Debug.Assert(entity.HasAttribute("mesh"));
            Debug.Assert(entity.HasAttribute("position"));

            Vector3 position = entity.GetVector3("position");
            Vector3 scale = new Vector3(1, 1, 1);
            if (entity.HasAttribute("scale"))
            {
                scale = entity.GetVector3("scale");
            }

            Matrix world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Look up the effect, and set effect parameters on it. This sample
                    // assumes the model will only be using BasicEffect, but a more robust
                    // implementation would probably want to handle custom effects as well.
                    BasicEffect effect = (BasicEffect)part.Effect;

                    effect.EnableDefaultLighting();

                    effect.World = world;
                    effect.View = Game.Instance.View;
                    effect.Projection = Game.Instance.Projection;

                    // Set the graphics device to use our vertex declaration,
                    // vertex buffer, and index buffer.
                    GraphicsDevice device = effect.GraphicsDevice;

                    device.VertexDeclaration = part.VertexDeclaration;

                    device.Vertices[0].SetSource(mesh.VertexBuffer, 0,
                                                 part.VertexStride);

                    device.Indices = mesh.IndexBuffer;

                    // Begin the effect, and loop over all the effect passes.
                    effect.Begin();

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Begin();

                        // Draw the geometry.
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                                     part.BaseVertex, 0, part.NumVertices,
                                                     part.StartIndex, part.PrimitiveCount);

                        pass.End();
                    }

                    effect.End();
                }
            }
        }

        private Model model;
    }
}
