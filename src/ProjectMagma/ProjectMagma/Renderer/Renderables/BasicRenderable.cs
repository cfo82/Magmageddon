using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectMagma.Renderer
{
    public class BasicRenderable : ModelRenderable
    {
        public BasicRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(scale, rotation, position, model)
        {
            start_squash = false;
            last_squash_start = -10000;
            UseLights = true;
            UseMaterialParameters = true;
            UseSquash = true;
            RenderChannel = RenderChannelType.Three;
        }


        protected override void DrawMesh(Renderer renderer, GameTime gameTime, ModelMesh mesh)
        {
            if(start_squash)
            {
                last_squash_start = gameTime.TotalRealTime.TotalMilliseconds;
                start_squash = false;
            }
            foreach (Effect effect in mesh.Effects)
            {
                // set shader parameters
                ApplyRenderChannel(effect);
                ApplyWorldViewProjection(effect, mesh);
                ApplyTechnique(effect);
                if (UseLights) ApplyLights(effect, renderer.LightManager);
                if (UseMaterialParameters) ApplyMaterialParameters(effect);
                if (UseSquash) ApplySquashParameters(effect, gameTime);
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
            Effect effect = Game.Instance.Content.Load<Effect>("Effects/Basic/Basic");
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

        protected void ApplyLights(Effect effect, LightManager lightManager)
        {
            effect.Parameters["DirLight0DiffuseColor"].SetValue(lightManager.SkyLight.DiffuseColor * SkyLightStrength);
            effect.Parameters["DirLight0SpecularColor"].SetValue(lightManager.SkyLight.SpecularColor * SkyLightStrength);
            effect.Parameters["DirLight0Direction"].SetValue(lightManager.SkyLight.Direction * SkyLightStrength);

            effect.Parameters["DirLight1DiffuseColor"].SetValue(lightManager.LavaLight.DiffuseColor * LavaLightStrength);
            effect.Parameters["DirLight1SpecularColor"].SetValue(lightManager.LavaLight.SpecularColor * LavaLightStrength);
            effect.Parameters["DirLight1Direction"].SetValue(lightManager.LavaLight.Direction * LavaLightStrength);

            effect.Parameters["DirLight2DiffuseColor"].SetValue(lightManager.SpotLight.DiffuseColor * SpotLightStrength);
            effect.Parameters["DirLight2SpecularColor"].SetValue(lightManager.SpotLight.SpecularColor * SpotLightStrength);
            effect.Parameters["DirLight2Direction"].SetValue(lightManager.SpotLight.Direction * SpotLightStrength);
        }

        protected virtual void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            effect.Parameters["Clouds"].SetValue(renderer.VectorCloudTexture);
            // in the end, this method in BasicRenderable should be empty and all the features
            // be implemented in individual methods which are called by their name

            //effect.Parameters["FogEnabled"].SetValue(0.0f);
            //effect.Parameters["FogStart"].SetValue(1000.0f);
            //effect.Parameters["FogEnd"].SetValue(2000.0f);
            //effect.Parameters["FogColor"].SetValue(Vector3.One);
            //effect.Parameters["EyePosition"].SetValue(Game.Instance.EyePosition);
        }

        private void ApplySquashParameters(Effect effect, GameTime gameTime)
        {
            double time_since_last_squash = gameTime.TotalRealTime.TotalMilliseconds - last_squash_start;
            int t = 170;
            float a = 0.2f;
            if (time_since_last_squash > 0 && time_since_last_squash <= t / 2)
                effect.Parameters["SquashAmount"].SetValue((float)time_since_last_squash / t * a * 2);
            else if (time_since_last_squash >= t / 2 && time_since_last_squash <= t)
                effect.Parameters["SquashAmount"].SetValue((float)(t - time_since_last_squash) / t * a * 2);
            else
                effect.Parameters["SquashAmount"].SetValue(0.0f);
        }

        private void ApplyMaterialParameters(Effect effect)
        {
            effect.Parameters["DiffuseColor"].SetValue(DiffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(EmissiveColor);
            effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            effect.Parameters["Alpha"].SetValue(Alpha);
            effect.Parameters["SpecularPower"].SetValue(SpecularPower);
        }

        public void Squash()
        {
            start_squash = true;
        }

        private bool start_squash;
        private double last_squash_start;

        public Vector3 DiffuseColor { get; set; }
        public Vector3 EmissiveColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float Alpha { get; set; }
        public float SpecularPower { get; set; }

        protected bool UseLights { get; set; }
        protected bool UseMaterialParameters { get; set; }
        protected bool UseSquash { get; set; }
    }
}
