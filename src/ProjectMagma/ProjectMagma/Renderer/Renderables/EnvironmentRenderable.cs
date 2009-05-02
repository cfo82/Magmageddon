using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;

namespace ProjectMagma.Renderer
{
    public class EnvironmentRenderable : TexturedRenderable
    {
        public EnvironmentRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model, Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
            : base(scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture)
        {
            randomOffset = new DoublyIntegratedVector2
            (
                Vector2.Zero, new Vector2(0.0002f, 0.0002f), 0.0f, 0.0f, -0.0004f, 0.0004f
            );
            //EnvGroundWavesAmplitude = 15.0f;
            //EnvGroundWavesFrequency = 0.002f;
            //EnvGroundWavesHardness = 5.5f;
            //EnvGroundWavesVelocity = 0.003f;
            RenderChannel = RenderChannelType.One;
        }

        //protected override void ApplyEffectsToModel()
        //{
        //    Effect effect = Game.Instance.ContentManager.Load<Effect>("Effects/Environment/Island");
        //    SetDefaultMaterialParameters();
        //    SetModelEffect(Model, effect);
        //}
        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Environment"];
        }

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer, GameTime gameTime)
        {
            base.ApplyCustomEffectParameters(effect, renderer, gameTime);

            randomOffset.RandomlyIntegrate(gameTime, EnvGroundWavesVelocity, 0.0f);
            effect.Parameters["RandomOffset"].SetValue(randomOffset.Value);

            effect.Parameters["EnvGroundWavesAmplitude"].SetValue(EnvGroundWavesAmplitude);
            effect.Parameters["EnvGroundWavesFrequency"].SetValue(EnvGroundWavesFrequency);
            effect.Parameters["EnvGroundWavesHardness"].SetValue(EnvGroundWavesHardness);

            effect.Parameters["DirLight1BottomAmpMaxY"].SetValue(300);
            effect.Parameters["DirLight1BottomAmpStrength"].SetValue(3);
            //effect.Parameters["EnvGroundWavesHardness"].SetValue(EnvGroundWavesHardness);
        }

        public override void UpdateFloat(string id, float value)
        {
            base.UpdateFloat(id, value);

            if (id == "EnvGroundWavesAmplitude")
            {
                EnvGroundWavesAmplitude = value;
            }
            else if (id == "EnvGroundWavesFrequency")
            {
                EnvGroundWavesFrequency = value;
            }
            else if (id == "EnvGroundWavesHardness")
            {
                EnvGroundWavesHardness = value;
            }
            else if (id == "EnvGroundWavesVelocity")
            {
                EnvGroundWavesVelocity = value;
            }
        }
        
        private DoublyIntegratedVector2 randomOffset;

        private float EnvGroundWavesAmplitude { get; set; }
        private float EnvGroundWavesFrequency { get; set; }
        private float EnvGroundWavesHardness { get; set; }
        private float EnvGroundWavesVelocity { get; set; }
    }
}
