﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class TexturedRenderProperty : BasicRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            Texture2D diffuseTexture = null;
            Texture2D specularTexture = null;
            Texture2D normalTexture = null;

            if (entity.HasString("diffuse_texture"))
            {
                string textureName = entity.GetString("diffuse_texture");
                diffuseTexture = Game.Instance.ContentManager.Load<Texture2D>(textureName);
            }
            if (entity.HasString("specular_texture"))
            {
                string textureName = entity.GetString("specular_texture");
                specularTexture = Game.Instance.ContentManager.Load<Texture2D>(textureName);
            }
            if (entity.HasString("normal_texture"))
            {
                string textureName = entity.GetString("normal_texture");
                normalTexture = Game.Instance.ContentManager.Load<Texture2D>(textureName);
            }

            return CreateTexturedRenderable(entity, renderPriority, scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture);
        }

        protected virtual TexturedRenderable CreateTexturedRenderable(
            Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model,
            Texture2D diffuseTexture, Texture2D specularTexture, Texture2D normalTexture)
        {
            return new TexturedRenderable(Game.Instance.Simulation.Time.At, renderPriority, scale, rotation, position, model, diffuseTexture, specularTexture, normalTexture);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            base.SetUpdatableParameters(entity);
        }
    }
}
