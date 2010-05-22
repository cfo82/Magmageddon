using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;

// def. environment: pillars, cave
namespace ProjectMagma.Simulation
{
    public class EnvironmentRenderProperty : TexturedRenderProperty
    {
        protected override TexturedRenderable CreateTexturedRenderable(
            Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
        {
            return new EnvironmentRenderable(
                Game.Instance.Simulation.Time.At, renderPriority,
                scale, rotation, position, model,
                diffuseTexture, specularTexture, normalTexture);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            base.SetUpdatableParameters(entity);

            if (entity.HasFloat(CommonNames.EnvGroundWavesAplitude))
            {
                ChangeFloat("EnvGroundWavesAmplitude", entity.GetFloat(CommonNames.EnvGroundWavesAplitude));
            }
            if (entity.HasFloat(CommonNames.EnvGroundWavesFrequency))
            {
                ChangeFloat("EnvGroundWavesFrequency", entity.GetFloat(CommonNames.EnvGroundWavesFrequency));
            }
            if (entity.HasFloat(CommonNames.EnvGroundWavesHardness))
            {
                ChangeFloat("EnvGroundWavesHardness", entity.GetFloat(CommonNames.EnvGroundWavesHardness));
            }
            if (entity.HasFloat(CommonNames.EnvGroundWavesVelocity))
            {
                ChangeFloat("EnvGroundWavesVelocity", entity.GetFloat(CommonNames.EnvGroundWavesVelocity));
            }
        }
    }
}
