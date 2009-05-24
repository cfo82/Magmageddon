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
        public EnvironmentRenderable(double timestamp, Vector3 scale, Quaternion rotation, Vector3 position, Model model, Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
        :   base(timestamp, scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture)
        {
            randomOffset = new DoublyIntegratedVector2
            (
                Vector2.Zero, new Vector2(0.0002f, 0.0002f), 0.0f, 0.0f, -0.0004f, 0.0004f
            );
            RenderChannel = RenderChannelType.Three;
        }

        protected override void ApplyTechnique(Effect effect)
        {
            effect.CurrentTechnique = effect.Techniques["Environment"];
        }

        protected override void ApplyCustomEffectParameters(Effect effect, Renderer renderer)
        {
            base.ApplyCustomEffectParameters(effect, renderer);

            randomOffset.RandomlyIntegrate(renderer.Time.DtMs, EnvGroundWavesVelocity, 0.0f);
            effect.Parameters["RandomOffset"].SetValue(randomOffset.Value);

            effect.Parameters["EnvGroundWavesAmplitude"].SetValue(EnvGroundWavesAmplitude);
            effect.Parameters["EnvGroundWavesFrequency"].SetValue(EnvGroundWavesFrequency);
            effect.Parameters["EnvGroundWavesHardness"].SetValue(EnvGroundWavesHardness);

            effect.Parameters["DirLight1BottomAmpMaxY"].SetValue(renderer.EntityManager["environment"].GetFloat("dir_light_1_bottom_amp_max_y"));
            effect.Parameters["DirLight1MinMultiplier"].SetValue(renderer.EntityManager["environment"].GetFloat("dir_light_1_min_multiplier"));
            effect.Parameters["DirLight1MaxMultiplier"].SetValue(renderer.EntityManager["environment"].GetFloat("dir_light_1_max_multiplier"));
            effect.Parameters["CutLight1"].SetValue(1.0f);
            //effect.Parameters["DirLight1BottomAmpStrength"].SetValue(3);
        }

        public override void UpdateFloat(string id, double timestamp, float value)
        {
            base.UpdateFloat(id, timestamp, value);

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
