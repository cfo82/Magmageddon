﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class TexturedRenderProperty : BasicRenderProperty
    {
        protected override ModelRenderable CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model)
        {
            return new TexturedRenderable(scale, rotation, position, model);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            base.SetUpdatableParameters(entity);

            if (entity.HasString("diffuse_texture"))
            {
                string textureName = entity.GetString("diffuse_texture");
                Texture2D texture = Game.Instance.Content.Load<Texture2D>(textureName);
                (Updatable as TexturedRenderable).DiffuseTexture = texture;
            }
            if (entity.HasString("specular_texture"))
            {
                string textureName = entity.GetString("specular_texture");
                Texture2D texture = Game.Instance.Content.Load<Texture2D>(textureName);
                (Updatable as TexturedRenderable).SpecularTexture = texture;
            }
            if (entity.HasString("normal_texture"))
            {
                string textureName = entity.GetString("normal_texture");
                Texture2D texture = Game.Instance.Content.Load<Texture2D>(textureName);
                (Updatable as TexturedRenderable).NormalTexture = texture;
            }
        }
    }
}
