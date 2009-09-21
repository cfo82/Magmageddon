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
            int renderPriority,
            Vector3 scale,
            Quaternion rotation,
            Vector3 position,
            Model model
        )
        {
            this.renderPriority = renderPriority;
            this.Scale = scale;
            this.rotation = new QuaternionInterpolationHistory(timestamp, rotation);
            this.position = new Vector3InterpolationHistory(timestamp, position);
            this.Model = model;
            this.boneTransforms = new Matrix[Model.Bones.Count];
            SkyLightStrength = 1.0f;
            LavaLightStrength = 1.0f;
            SpotLightStrength = 1.0f;
            RenderChannel = RenderChannelType.One;
            IsShadowCaster = true;
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
            }
        }

        public virtual void DrawToShadowMap(Renderer renderer)
        {
            // TODO: should only be done once per frame.
            RecomputeWorldMatrix();
            RecomputeBoneTransforms();

//            model.CopyAbsoluteBoneTransformsTo(transforms);

            // shadows should be floating a little above the receiving surface

            foreach (ModelMesh mesh in Model.Meshes)
            {
                //ApplyMeshWorldViewProjection(renderer, renderer.ShadowEffect, mesh);
                Matrix world_offset = World;
                world_offset *= Matrix.CreateTranslation(new Vector3(0, 3, 0));
                
                Effect effect = renderer.ShadowEffect;
                
                // hack
                if (this is BasicRenderable)
                {
                    BasicRenderable this_basic = this as BasicRenderable;
                    if (this_basic.UseSquash) this_basic.ApplySquashParameters(effect, renderer);
                }
                // endhack
                renderer.Device.RenderState.DepthBufferEnable = true;
                effect.CurrentTechnique = effect.Techniques["DepthMap"];
                effect.Parameters["LightPosition"].SetValue(renderer.LightPosition);

                effect.Parameters["Local"].SetValue(BoneTransformMatrix(mesh));
                effect.Parameters["World"].SetValue(world_offset);

                //world_offset *= Matrix.CreateTranslation(new Vector3(0, 3, 0));

                //effect.Parameters["WorldLightViewProjection"].SetValue(
                //    world_offset * renderer.LightView * renderer.LightProjection);

                effect.Parameters["LightViewProjection"].SetValue(
                    renderer.LightView * renderer.LightProjection);


                Effect backup = mesh.MeshParts[0].Effect;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
                renderer.Device.RenderState.AlphaBlendEnable = false;
                renderer.Device.RenderState.SourceBlend = Blend.SourceAlpha;
                renderer.Device.RenderState.DestinationBlend = Blend.DestinationColor;

                mesh.Draw();

                renderer.Device.RenderState.AlphaBlendEnable = false;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = backup;
                }
            }
        }

        protected virtual void DrawMesh(Renderer renderer, ModelMesh mesh)
        {
            mesh.Draw();
        }

        protected void ApplyMeshWorldViewProjection(Renderer renderer, Effect effect, ModelMesh mesh)
        {
            effect.Parameters["Local"].SetValue(BoneTransformMatrix(mesh));
            ApplyWorldViewProjection(renderer, effect);
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

            switch (id)
            {
                case "LavaLightStrength":
                    LavaLightStrength = value;
                    break;
                case "SkyLightStrength":
                    SkyLightStrength = value;
                    break;
                case "SpotLightStrength":
                    SpotLightStrength = value;
                    break;
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

        public override int RenderPriority
        {
            get { return renderPriority; }
        }
        
        protected Model Model { get; set; }
        virtual protected Matrix World { get; set; }

        public float SkyLightStrength { get; set; }
        public float LavaLightStrength { get; set; }
        public float SpotLightStrength { get; set; }

        public enum RenderChannelType { One, Two, Three };
        public RenderChannelType RenderChannel { get; set; }

        PartEffectMapping defaultEffectMapping;

        private Vector3InterpolationHistory position;
        private QuaternionInterpolationHistory rotation;

        private Matrix[] boneTransforms;

        public bool IsShadowCaster { get; set; }

        public int renderPriority;
    }
}
