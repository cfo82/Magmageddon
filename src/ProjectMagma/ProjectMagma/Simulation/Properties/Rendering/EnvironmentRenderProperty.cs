using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

// def. environment: pillars, cave
namespace ProjectMagma.Simulation
{
    public class EnvironmentRenderProperty : TexturedRenderProperty
    {
        protected override TexturedRenderable CreateTexturedRenderable(
            Entity entity, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
        {
            return new EnvironmentRenderable(scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            base.SetUpdatableParameters(entity);

            if (entity.HasFloat("env_ground_waves_amplitude"))
            {
                ChangeFloat("EnvGroundWavesAmplitude", entity.GetFloat("env_ground_waves_amplitude"));
            }
            if (entity.HasFloat("env_ground_waves_frequency"))
            {
                ChangeFloat("EnvGroundWavesFrequency", entity.GetFloat("env_ground_waves_frequency"));
            }
            if (entity.HasFloat("env_ground_waves_hardness"))
            {
                ChangeFloat("EnvGroundWavesHardness", entity.GetFloat("env_ground_waves_hardness"));
            }
            if (entity.HasFloat("env_ground_waves_velocity"))
            {
                ChangeFloat("EnvGroundWavesVelocity", entity.GetFloat("env_ground_waves_velocity"));
            }
        }
    }
}
