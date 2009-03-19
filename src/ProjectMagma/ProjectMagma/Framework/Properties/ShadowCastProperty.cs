﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Framework
{
    public class ShadowCastProperty : Property
    {
        public ShadowCastProperty()
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

                switch(renderMode)
                {
                    case RenderMode.RenderToScene:
                        // nothing to do here
                        //break;                        
                        return;
                    case RenderMode.RenderToShadowMap:
                        Game.Instance.Graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
                        effect.CurrentTechnique = effect.Techniques["DepthMap"];
                        effect.Parameters["LightPosition"].SetValue(Game.Instance.lightPosition);
                        effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * world);
                        break;
                    case RenderMode.RenderToSceneAlpha:
                        // nothing to do here
                        return;
                        //break;                
                    default:
                        Debug.Assert(false, "unhandled render mode in shadow cast property."); // HACK: maybe do better error handling?
                        break;
                }

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
        private Model model;
    }
}