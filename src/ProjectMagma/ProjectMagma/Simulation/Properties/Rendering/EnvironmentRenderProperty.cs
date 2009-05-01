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
        protected override ModelRenderable CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new EnvironmentRenderable(scale, rotation, position, model);
        }

        protected override void SetRenderableParameters(Entity entity)
        {
            base.SetRenderableParameters(entity);

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
