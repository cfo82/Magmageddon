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
            if (renderMode == RenderMode.RenderToScene)
            {
                Debug.Assert(entity.HasAttribute("mesh"));
                Debug.Assert(entity.HasAttribute("position"));

                Matrix world = Matrix.Identity;

                #region compute world matrix

                // scaling
                if (entity.HasVector3("scale"))
                {
                    Vector3 scale = entity.GetVector3("scale");
                    world *= Matrix.CreateScale(scale);
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

                #endregion

                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);

                // shadows should be floating a little above the receiving surface
                Matrix world_offset = world;
                world_offset *= Matrix.CreateTranslation(new Vector3(0, 3, 0));

                foreach (ModelMesh mesh in model.Meshes)
                {
                    Effect effect = Game.Instance.shadowEffect;

                    Game.Instance.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    foreach (BasicEffect effectx in mesh.Effects)
                    {
                        effectx.EnableDefaultLighting();
                        effectx.View = Game.Instance.View;
                        effectx.Projection = Game.Instance.Projection;
                        effectx.World = transforms[mesh.ParentBone.Index] * world;
                    }
                    mesh.Draw();
                    Game.Instance.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;

                    effect.CurrentTechnique = effect.Techniques["Scene"];
                    effect.Parameters["ShadowMap"].SetValue(Game.Instance.lightResolve);
                    effect.Parameters["WorldCameraViewProjection"].SetValue(
                        transforms[mesh.ParentBone.Index] * world_offset * Game.Instance.View * Game.Instance.Projection);
                    effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world_offset);

                    effect.Parameters["WorldLightViewProjection"].SetValue(
                        transforms[mesh.ParentBone.Index] * world_offset * Game.Instance.lightView * Game.Instance.lightProjection);


                    Effect backup = mesh.MeshParts[0].Effect;
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = effect;
                    }
                    Game.Instance.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    Game.Instance.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    Game.Instance.GraphicsDevice.RenderState.DestinationBlend = Blend.DestinationColor;

                    mesh.Draw();

                    Game.Instance.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        meshPart.Effect = backup;
                    }
                }
            }
        }

        private Model model;
    }
}
