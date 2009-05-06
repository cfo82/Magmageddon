using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;

namespace ProjectMagma.Renderer
{
    public abstract class ModelRenderable : Renderable
    {
        #region constructor

        public ModelRenderable(
            Vector3 scale,
            Quaternion rotation,
            Vector3 position,
            Model model
        )
        {
            this.Scale = scale;
            this.Rotation = rotation;
            this.Position = position;
            this.Model = model;
            this.boneTransforms = new Matrix[Model.Bones.Count];
            SkyLightStrength = 1.0f;
            LavaLightStrength = 1.0f;
            SpotLightStrength = 1.0f;
            RenderChannel = RenderChannelType.One;
            ApplyEffectsToModel();
            defaultEffectMapping = CurrentPartEffectMapping();
        }

        public override void LoadResources()
        {
            base.LoadResources();
        }

        #endregion

        #region recomputations

        public void RecomputeWorldMatrix()
        {
            World = Matrix.Identity;
            World *= Matrix.CreateScale(Scale);
            World *= Matrix.CreateFromQuaternion(Rotation);
            World *= Matrix.CreateTranslation(Position);
        }

        protected void RecomputeBoneTransforms()
        {
            Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
        }

        #endregion

        protected abstract void ApplyEffectsToModel();

        public override void Draw(Renderer renderer, GameTime gameTime)
        {
            RecomputeWorldMatrix();
            RecomputeBoneTransforms();
            foreach (ModelMesh mesh in Model.Meshes)
            {
                PrepareMeshEffects(renderer, gameTime, mesh);
                DrawMesh(renderer, gameTime, mesh);
                DrawShadow(ref renderer, mesh);
            }
        }

        protected virtual void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {
            mesh.Draw();
        }

        protected sealed class PartEffectMapping : Dictionary<ModelMeshPart, Effect> { }

        protected PartEffectMapping CurrentPartEffectMapping()
        {
            PartEffectMapping mapping = new PartEffectMapping();
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    mapping[meshPart] = meshPart.Effect;
                }
            }
            return mapping;
        }

        protected void RestorePartEffectMapping(PartEffectMapping mapping)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = mapping[meshPart];
                }
            }
        }


        protected void ReplaceBasicEffect(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                ReplaceBasicEffect(mesh, effect);
            }
        }


        protected void ReplaceBasicEffect(ModelMesh mesh, Effect effect)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                if (meshPart.Effect is BasicPaletteEffect || meshPart.Effect is BasicEffect)
                {
                    Effect oldEffect = meshPart.Effect;
                    meshPart.Effect = effect.Clone(oldEffect.GraphicsDevice);
                    oldEffect.Dispose();
                }
            }
        }

        protected void SetEffect(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                SetEffect(mesh, effect);
            }
        }
        
        protected void SetEffect(ModelMesh mesh, Effect effect)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                //Effect oldEffect = meshPart.Effect;
                meshPart.Effect = effect;//.Clone(oldEffect.GraphicsDevice);
                //oldEffect.Dispose();
            }
        }

        protected void SetGlobalEffectParameter(string name, Vector3 value)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach(Effect effect in mesh.Effects)
                {
                    effect.Parameters[name].SetValue(value);
                }
            }
        }

        protected void SetGlobalEffectParameter(string name, float value)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters[name].SetValue(value);
                }
            }
        }

        protected abstract void PrepareMeshEffects(Renderer renderer, GameTime gameTime, ModelMesh mesh);

        private void DrawShadow(ref Renderer renderer, ModelMesh mesh)
        {
            Effect shadowEffect = renderer.ShadowEffect;

            // shadows should be floating a little above the receiving surface
            Matrix world_offset = BoneTransformMatrix(mesh) * World * Matrix.CreateTranslation(new Vector3(0, 3, 0));

            shadowEffect.CurrentTechnique = shadowEffect.Techniques["Scene"];
            shadowEffect.Parameters["ShadowMap"].SetValue(renderer.LightResolve);
            shadowEffect.Parameters["WorldCameraViewProjection"].SetValue(
                world_offset * renderer.Camera.View * renderer.Camera.Projection);
            shadowEffect.Parameters["World"].SetValue(world_offset);

            shadowEffect.Parameters["WorldLightViewProjection"].SetValue(
                world_offset * renderer.LightView * renderer.LightProjection);

            //ReplaceBasicEffect(mesh, shadowEffect);
            SetEffect(mesh, shadowEffect);
            mesh.Draw();
            RestorePartEffectMapping(defaultEffectMapping);
        }


        protected void ApplyRenderChannel(Effect effect)
        {
            Vector4 RenderChannelColor = Vector4.Zero;
            switch(RenderChannel)
            {
                case RenderChannelType.One:
                    RenderChannelColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    break;
                case RenderChannelType.Two:
                    RenderChannelColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                    break;
                case RenderChannelType.Three:
                    RenderChannelColor = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                    break;                
            }
            Debug.Assert(RenderChannelColor != Vector4.Zero);
            effect.Parameters["RenderChannelColor"].SetValue(RenderChannelColor);
        }

        protected Matrix BoneTransformMatrix(ModelMesh mesh)
        {
            return boneTransforms[mesh.ParentBone.Index];
        }

        public override RenderMode RenderMode 
        {
            get { return RenderMode.RenderToScene; }
        }

        private Matrix[] boneTransforms;

        public override void UpdateFloat(string id, float value)
        {
            base.UpdateFloat(id, value);

            if (id == "LavaLightStrength")
            {
                LavaLightStrength = value;
            }
            else if (id == "SkyLightStrength")
            {
                SkyLightStrength = value;
            }
            else if (id == "SpotLightStrength")
            {
                SpotLightStrength = value;
            }
        }

        public override void UpdateQuaternion(string id, Quaternion value)
        {
            base.UpdateQuaternion(id, value);

            if (id == "Rotation")
            {
                Rotation = value;
            }
        }

        public override void UpdateVector3(string id, Vector3 value)
        {
            base.UpdateVector3(id, value);

            if (id == "Position")
            {
                Position = value;
            }
            else if (id == "Scale")
            {
                Scale = value;
            }
        }

        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }
        public override Vector3 Position { get; set; }
        
        protected Model Model { get; set; }
        protected Matrix World { get; set; }

        public float SkyLightStrength { get; set; }
        public float LavaLightStrength { get; set; }
        public float SpotLightStrength { get; set; }

        public enum RenderChannelType { One, Two, Three };
        public RenderChannelType RenderChannel { get; set; }

        PartEffectMapping defaultEffectMapping;
    }
}
