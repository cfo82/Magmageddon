using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class ShadowCastProperty : Property
    {
        public ShadowCastProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            entity.Draw += OnDraw;

            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            if (entity.HasVector3("scale"))
            {
                scale = entity.GetVector3("scale");
                entity.GetVector3Attribute("scale").ValueChanged += ScaleChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                rotation = entity.GetQuaternion("rotation");
                entity.GetQuaternionAttribute("rotation").ValueChanged += RotationChanged;
            }
            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            // load the model
            string meshName = entity.GetString("mesh");
            model = Game.Instance.Content.Load<Model>(meshName);

            renderable = new ShadowRenderable(scale, rotation, position, model);
        }

        public void OnDetached(Entity entity)
        {
            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged -= PositionChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged -= RotationChanged;
            }
            if (entity.HasVector3("scale"))
            {
                entity.GetVector3Attribute("scale").ValueChanged -= ScaleChanged;
            }
            entity.Draw -= OnDraw;
        }

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Scale = newValue;
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            renderable.Rotation = newValue;
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Position = newValue;
        }

        private void OnDraw(Entity entity, GameTime gameTime, RenderMode renderMode)
        {
            if (renderMode == RenderMode.RenderToShadowMap)
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

                    Game.Instance.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    effect.CurrentTechnique = effect.Techniques["DepthMap"];
                    effect.Parameters["LightPosition"].SetValue(Game.Instance.lightPosition);
                    effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world);

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

        private ShadowRenderable renderable;
        private Model model;
    }
}
