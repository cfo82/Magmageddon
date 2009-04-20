using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class BasicRenderable : ModelRenderable
    {
        public BasicRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model) { }

        protected override void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {
            foreach (Effect effect in mesh.Effects)
            {
                // set shader parameters
                ApplyRenderChannel(effect);
                ApplyWorldViewProjection(effect, mesh);
                ApplyTechnique(effect);
                ApplyLights(effect, renderer.LightManager);
                ApplyMaterialParameters(effect);
                ApplyCustomEffectParameters(effect, renderer, gameTime);
            }
            mesh.Draw();
        }

        private void ApplyWorldViewProjection(Effect effect, ModelMesh mesh)
        {
            effect.Parameters["World"].SetValue(BoneTransformMatrix(mesh) * World);
            effect.Parameters["View"].SetValue(Game.Instance.View);
            effect.Parameters["Projection"].SetValue(Game.Instance.Projection);
        }
        
        protected virtual void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Unicolored"];
        }

        protected override void ApplyEffectsToModel()
        {
            Effect effect = Game.Instance.Content.Load<Effect>("Effects/Basic");
            SetDefaultMaterialParameters();
            SetModelEffect(Model, effect);
        }

        protected virtual void SetDefaultMaterialParameters()
        {
            Alpha = 1.0f;
            SpecularPower = 16.0f;
            DiffuseColor = Vector3.One * 0.5f;
            SpecularColor = Vector3.Zero;
            EmissiveColor = Vector3.Zero;
        }

        protected virtual void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            //effect.Parameters["FogEnabled"].SetValue(1.0f);
            //effect.Parameters["FogStart"].SetValue(1000.0f);
            //effect.Parameters["FogEnd"].SetValue(2000.0f);
            //effect.Parameters["FogColor"].SetValue(Vector3.One);
            //effect.Parameters["EyePosition"].SetValue(Game.Instance.EyePosition);
        }

        private void ApplyMaterialParameters(Effect effect)
        {
            effect.Parameters["DiffuseColor"].SetValue(DiffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(EmissiveColor);
            effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            effect.Parameters["Alpha"].SetValue(Alpha);
            effect.Parameters["SpecularPower"].SetValue(SpecularPower);
        }

        public Vector3 DiffuseColor { get; set; }
        public Vector3 EmissiveColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float Alpha { get; set; }
        public float SpecularPower { get; set; }     
    }
}
