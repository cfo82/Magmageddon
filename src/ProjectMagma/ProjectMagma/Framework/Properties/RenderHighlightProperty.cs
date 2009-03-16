using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    public class RenderHighlightProperty : Property
    {
        public RenderHighlightProperty()
        {
            model = null;
        }

        public void OnAttached(Entity entity)
        {
            entity.Draw += OnDraw;

            // load the model
            string meshName = entity.GetString("mesh");
            model = Game.Instance.Content.Load<Model>(meshName);
        }

        public void OnDetached(Entity entity)
        {
            entity.Draw -= OnDraw;
        }

        private void OnDraw(Entity entity, GameTime gameTime, RenderMode renderMode)
        {
            if (renderMode == RenderMode.RenderToSceneAlpha)
            {
                Debug.Assert(entity.HasAttribute("mesh"));
                Debug.Assert(entity.HasAttribute("position"));

                Matrix world = Matrix.Identity;

                // scaling
                if (entity.HasVector3("scale"))
                {
                    Vector3 scale = entity.GetVector3("scale");
                    scale.X += 1.1f;
                    scale.Y += 1.1f;
                    scale.Z += 1.1f;
                    world *= Matrix.CreateScale(scale);
                }
                else
                {
                    world *= Matrix.CreateScale(new Vector3(1.1f, 1.1f, 1.1f));
                }

                // y rotation (if we need other rotations, these are yet to be added)
                if (entity.HasQuaternion("rotation"))
                {
                    Quaternion rotation = entity.GetQuaternion("rotation");
                    world *= Matrix.CreateFromQuaternion(rotation);
                }

                // translation
                Vector3 position = entity.GetVector3("position");
                world *= Matrix.CreateTranslation(position);

                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);

                foreach (ModelMesh mesh in model.Meshes)
                {
                    Effect effect = Game.Instance.shadowEffect;

                    Game.Instance.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    Vector3[] diffuseColors = new Vector3[mesh.Effects.Count];
                    int i = 0;
                    foreach (BasicEffect effectx in mesh.Effects)
                    {
                        diffuseColors[i] = effectx.DiffuseColor;
                        effectx.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
                        effectx.EnableDefaultLighting();
                        effectx.View = Game.Instance.View;
                        effectx.Projection = Game.Instance.Projection;
                        effectx.World = transforms[mesh.ParentBone.Index] * world;
                        ++i;
                    }
                    mesh.Draw();

                    i = 0;
                    Effect[] effectBackup = new Effect[mesh.MeshParts.Count];
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        effectBackup[i] = meshPart.Effect;
                        meshPart.Effect = effect;
                        ++i;
                    }
                    Game.Instance.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    Game.Instance.GraphicsDevice.RenderState.BlendFactor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
                    Game.Instance.GraphicsDevice.RenderState.SourceBlend = Blend.BlendFactor;
                    Game.Instance.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
                    Game.Instance.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseBlendFactor;

                    mesh.Draw();

                    Game.Instance.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    i = 0;
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = effectBackup[i];
                        ++i;
                    }
                    i = 0;
                    foreach (BasicEffect effectx in mesh.Effects)
                    {
                        effectx.DiffuseColor = diffuseColors[i];
                        ++i;
                    }
                }
            }
        }
        private Model model;
    }
}
