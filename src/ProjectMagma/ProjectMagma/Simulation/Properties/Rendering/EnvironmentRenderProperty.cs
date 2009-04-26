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
        public override void CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            Renderable = new EnvironmentRenderable(scale, rotation, position, model);
        }

        public override void SetRenderableParameters(Entity entity)
        {
            base.SetRenderableParameters(entity);

            if (entity.HasFloat("env_ground_waves_amplitude"))
            {
                (Renderable as EnvironmentRenderable).EnvGroundWavesAmplitude = entity.GetFloat("env_ground_waves_amplitude");
            }
            if (entity.HasFloat("env_ground_waves_frequency"))
            {
                (Renderable as EnvironmentRenderable).EnvGroundWavesFrequency = entity.GetFloat("env_ground_waves_frequency");
            }
            if (entity.HasFloat("env_ground_waves_hardness"))
            {
                (Renderable as EnvironmentRenderable).EnvGroundWavesHardness = entity.GetFloat("env_ground_waves_hardness");
            }
            if (entity.HasFloat("env_ground_waves_velocity"))
            {
                (Renderable as EnvironmentRenderable).EnvGroundWavesVelocity = entity.GetFloat("env_ground_waves_velocity");
            }
        }
    }
}
