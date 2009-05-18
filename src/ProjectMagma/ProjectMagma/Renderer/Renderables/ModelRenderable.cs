using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Renderer
{
    public abstract class ModelRenderable : Renderable
    {
        #region constructor

        public ModelRenderable(
            double timestamp,
            Vector3 scale,
            Quaternion rotation,
            Vector3 position,
            Model model
        )
        {
            this.Scale = scale;
            this.rotation = new QuaternionInterpolationHistory(timestamp, rotation);
            this.position = new Vector3InterpolationHistory(timestamp, position);
            this.Model = model;
            this.boneTransforms = new Matrix[Model.Bones.Count];
            SkyLightStrength = 1.0f;
            LavaLightStrength = 1.0f;
            SpotLightStrength = 1.0f;
            RenderChannel = RenderChannelType.One;
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);
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

        public override void Update(Renderer renderer)
        {
            base.Update(renderer);

            position.InvalidateUntil(renderer.Time.PausableAt);
            rotation.InvalidateUntil(renderer.Time.PausableAt);
        }

        public override void Draw(Renderer renderer)
        {
            ApplyEffectsToModel();
            defaultEffectMapping = CurrentPartEffectMapping();

            RecomputeWorldMatrix();
            RecomputeBoneTransforms();
            foreach (ModelMesh mesh in Model.Meshes)
            {
                PrepareMeshEffects(renderer, mesh);
                DrawMesh(renderer, mesh);
                DrawShadow(ref renderer, mesh);
            }
        }

        protected virtual void DrawMesh(Renderer renderer, ModelMesh mesh)
        {
            mesh.Draw();
        }

        protected void ApplyWorldViewProjection(Renderer renderer, Effect effect)
        {
            effect.Parameters["World"].SetValue(World);
            effect.Parameters["View"].SetValue(renderer.Camera.View);
            effect.Parameters["Projection"].SetValue(renderer.Camera.Projection);
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

        protected abstract void PrepareMeshEffects(Renderer renderer, ModelMesh mesh);

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


        protected void ApplyRenderChannel(Effect effect, RenderChannelType renderChannel)
        {
            Vector4 renderChannelColor = Vector4.Zero;
            switch(renderChannel)
            {
                case RenderChannelType.One:
                    renderChannelColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                    break;
                case RenderChannelType.Two:
                    renderChannelColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                    break;
                case RenderChannelType.Three:
                    renderChannelColor = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
                    break;                
            }
            Debug.Assert(renderChannelColor != Vector4.Zero);
            effect.Parameters["RenderChannelColor"].SetValue(renderChannelColor);
        }

        protected Matrix BoneTransformMatrix(ModelMesh mesh)
        {
            return boneTransforms[mesh.ParentBone.Index];
        }

        public override bool NeedsUpdate
        {
            get
            {
                return true;
            }
        }

        public override RenderMode RenderMode 
        {
            get { return RenderMode.RenderToScene; }
        }

        public override void UpdateFloat(string id, double timestamp, float value)
        {
            base.UpdateFloat(id, timestamp, value);

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

        public override void UpdateQuaternion(string id, double timestamp, Quaternion value)
        {
            base.UpdateQuaternion(id, timestamp, value);

            if (id == "Rotation")
            {
                rotation.AddKeyframe(timestamp, value);
            }
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "Position")
            {
                this.position.AddKeyframe(timestamp, value);
            }
            else if (id == "Scale")
            {
                Scale = value;
            }
        }

        public Vector3 Scale { get; set; }
        public Quaternion Rotation
        {
            get { return rotation.Evaluate(Game.Instance.Renderer.Time.PausableAt); }
        }
        public override Vector3 Position
        {
            get { return position.Evaluate(Game.Instance.Renderer.Time.PausableAt); }
        }
        
        protected Model Model { get; set; }
        protected Matrix World { get; set; }

        public float SkyLightStrength { get; set; }
        public float LavaLightStrength { get; set; }
        public float SpotLightStrength { get; set; }

        public enum RenderChannelType { One, Two, Three };
        public RenderChannelType RenderChannel { get; set; }

        PartEffectMapping defaultEffectMapping;

        private Vector3InterpolationHistory position;
        private QuaternionInterpolationHistory rotation;

        private Matrix[] boneTransforms;
    }
}
