﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ProjectMagma.MathHelpers;

namespace ProjectMagma.Renderer
{
    public class BasicRenderable : ModelRenderable
    {
        public BasicRenderable(double timestamp, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
            : base(timestamp, renderPriority, scale, rotation, position, model)
        {
            start_squash = false;
            start_blinking = false;
            last_squash_start = -100000;
            last_blinking_start = -100000;
            squash_wavelength = 170;
            squash_amplitude = 0.2f;
            BlinkingLength = 1000;
            // FIXME: tune blinking strength here. maybe use a configuration file?
            // replace 0.55f by whatever strength is needed and 25.0f with the interval
            blinkingState = new SineFloat(0.0f, 0.55f, 25.0f);
            UseLights = true;
            UseMaterialParameters = true;
            UseSquash = true;
            UseBlinking = true;
            RenderChannel = RenderChannelType.Three;
            if (PersistentSquash) start_squash = true;
            SetDefaultMaterialParameters();
        }

        public override void LoadResources(Renderer renderer)
        {
            base.LoadResources(renderer);
        }

        protected override void PrepareMeshEffects(Renderer renderer, ModelMesh mesh)
        {
            if(start_squash)
            {
                last_squash_start = renderer.Time.At;
                start_squash = false;
            }
            if (start_blinking)
            {
                last_blinking_start = renderer.Time.At;
                if (!blinkingState.Running)
                    { blinkingState.Start(renderer.Time.At); }
                start_blinking = false;
            }
            foreach (Effect effect in mesh.Effects)
            {
                // set shader parameters
                ApplyRenderChannel(effect, RenderChannel);
                ApplyMeshWorldViewProjection(renderer, effect, mesh);
                ApplyEyePosition(renderer, effect);
                ApplyTechnique(effect);
                ApplyShadowMap(renderer, effect);
                //ApplyFog(effect);
                if (UseLights) ApplyLights(effect, renderer.LightManager);
                if (UseMaterialParameters) ApplyMaterialParameters(effect);
                if (UseSquash) ApplySquashParameters(effect, renderer);
                if (UseBlinking) ApplyBlinkingParameters(effect, renderer);
                ApplyCustomEffectParameters(effect, renderer);
            }
        }

        private void ApplyShadowMap(Renderer renderer, Effect effect)
        {
            effect.Parameters["ShadowMap"].SetValue(renderer.LightRenderTarget);
            effect.Parameters["LightViewProjection"].SetValue(renderer.LightView * renderer.LightProjection);
        }

        
        protected void ApplyEyePosition(Renderer renderer, Effect effect)
        {
            effect.Parameters["EyePosition"].SetValue(renderer.Camera.Position);
        }

        protected virtual void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Unicolored"];
        }

        protected override void ApplyEffectsToModel()
        {
            Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Basic/Basic");
            ReplaceBasicEffect(Model, effect);
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
            effect.Parameters["DirLight0Direction"].SetValue(lightManager.SkyLight.Direction);

            effect.Parameters["DirLight1DiffuseColor"].SetValue(lightManager.LavaLight.DiffuseColor * LavaLightStrength);
            effect.Parameters["DirLight1SpecularColor"].SetValue(lightManager.LavaLight.SpecularColor * LavaLightStrength);
            effect.Parameters["DirLight1Direction"].SetValue(lightManager.LavaLight.Direction);

            effect.Parameters["DirLight2DiffuseColor"].SetValue(lightManager.SpotLight.DiffuseColor * SpotLightStrength);
            effect.Parameters["DirLight2SpecularColor"].SetValue(lightManager.SpotLight.SpecularColor * SpotLightStrength);
            effect.Parameters["DirLight2Direction"].SetValue(lightManager.SpotLight.Direction);
        }

        protected virtual void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
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

        public /* hack */ void ApplySquashParameters(Effect effect, Renderer renderer)
        {
            double time_since_last_squash = renderer.Time.At - last_squash_start;
            if (time_since_last_squash > 0 && time_since_last_squash <= squash_wavelength / 2)
                effect.Parameters["SquashAmount"].SetValue((float)time_since_last_squash / squash_wavelength * squash_amplitude * 2);
            else if (time_since_last_squash >= squash_wavelength / 2 && time_since_last_squash <= squash_wavelength)
                effect.Parameters["SquashAmount"].SetValue((float)(squash_wavelength - time_since_last_squash) / squash_wavelength * squash_amplitude * 2);
            else if (time_since_last_squash > squash_wavelength)
            {
                effect.Parameters["SquashAmount"].SetValue(0.0f);
                if (PersistentSquash)
                {
                    start_squash = true;
                }
            }
            else
            {   // time_since_last_squash <= 0
                // do nothing for now
            }
        }

        private void ApplyBlinkingParameters(Effect effect, Renderer renderer)
        {
            blinkingState.Update(renderer.Time.At);
            
            double blinkDelta = renderer.Time.At - last_blinking_start;
            if (blinkDelta > BlinkingLength)
            {
                blinkingState.StopAfterCurrentPhase();
            }

            effect.Parameters["BlinkingState"].SetValue(blinkingState.Value);
        }

        protected void ApplyMaterialParameters(Effect effect)
        {
            effect.Parameters["DiffuseColor"].SetValue(DiffuseColor);
            effect.Parameters["EmissiveColor"].SetValue(EmissiveColor);
            effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            effect.Parameters["Alpha"].SetValue(Alpha);
            effect.Parameters["SpecularPower"].SetValue(SpecularPower);
        }

        public override void UpdateBool(string id, double timestamp, bool value)
        {
            base.UpdateBool(id, timestamp, value);

            if (id == "Squash")
            {
                start_squash = value;
            }
            else if (id == "PersistentSquash")
            {
                PersistentSquash = value;
            }
            else if (id == "Blink")
            {
                start_blinking = value;
            }
        }

        public override void UpdateFloat(string id, double timestamp, float value)
        {
            base.UpdateFloat(id, timestamp, value);

            if (id == "SpecularPower")
            {
                SpecularPower = value;
            }
            else if (id == "Alpha")
            {
                Alpha = value;
            }
        }

        public override void UpdateVector2(string id, double timestamp, Vector2 value)
        {
            base.UpdateVector2(id, timestamp, value);

            if (id == "SquashParams")
            {
                SquashParams = value;
            }
        }

        public override void UpdateVector3(string id, double timestamp, Vector3 value)
        {
            base.UpdateVector3(id, timestamp, value);

            if (id == "DiffuseColor")
            {
                DiffuseColor = value;
            }
            else if (id == "EmissiveColor")
            {
                EmissiveColor = value;
            }
            else if (id == "SpecularColor")
            {
                SpecularColor = value;
            }
        }

        private bool start_squash, start_blinking;
        private double last_squash_start;
        private double last_blinking_start;
        private SineFloat blinkingState;

        private float squash_wavelength;
        protected float BlinkingLength { get; set; }
        private float squash_amplitude;
        protected Vector2 SquashParams
        {
            get { return new Vector2(squash_wavelength, squash_amplitude); }
            set { squash_wavelength = value.X; squash_amplitude = value.Y; }
        }
        protected bool PersistentSquash { get; set; }

        protected Vector3 DiffuseColor { get; set; }
        protected Vector3 EmissiveColor { get; set; }
        protected Vector3 SpecularColor { get; set; }
        protected float Alpha { get; set; }
        protected float SpecularPower { get; set; }

        protected bool UseLights { get; set; }
        protected bool UseMaterialParameters { get; set; }
        public bool UseSquash { get; set; }
        protected bool UseBlinking { get; set; }
    }
}
