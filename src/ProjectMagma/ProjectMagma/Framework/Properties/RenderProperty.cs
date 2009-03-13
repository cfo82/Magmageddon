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

        private void OnDraw(Entity entity, GameTime gameTime, RenderMode renderMode)
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

            foreach (ModelMesh mesh in model.Meshes)
            {
                Effect effect = Game.Instance.shadowEffect;

                
                //foreach (BasicEffect effect in mesh.Effects)
                //{

                    //effect.View = Game.Instance.View;
                    //effect.Projection = Game.Instance.Projection;
                    //effect.World = transforms[mesh.ParentBone.Index] * world;
                switch(renderMode)
                {
                    case RenderMode.RenderToScene:
                        effect.CurrentTechnique = effect.Techniques["Scene"];
                        effect.Parameters["ShadowMap"].SetValue(Game.Instance.lightResolve);
                        effect.Parameters["WorldCameraViewProjection"].SetValue(
                            transforms[mesh.ParentBone.Index] * world * Game.Instance.cameraView * Game.Instance.cameraProjection);
                        break;
                    case RenderMode.RenderToShadowMap:
                        effect.CurrentTechnique = effect.Techniques["DepthMap"];
                        effect.Parameters["LightPosition"].SetValue(Game.Instance.lightPosition);
                        break;
                    default:
                        Debug.Assert(false, "unhandled render mode."); // HACK: maybe do better error handling?
                        break;
                }

                effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world);
                effect.Parameters["WorldLightViewProjection"].SetValue(
                    transforms[mesh.ParentBone.Index] * world * Game.Instance.lightView * Game.Instance.lightProjection);

                Game.Instance.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
                mesh.Draw();
            }
        }
        private Model model;
    }
}
