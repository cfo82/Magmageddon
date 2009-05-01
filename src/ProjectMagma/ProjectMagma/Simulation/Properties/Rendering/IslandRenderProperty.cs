﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

// todo: this should inherit from basicrenderproperty
namespace ProjectMagma.Simulation
{
    public class IslandRenderProperty : Property
    {
        public IslandRenderProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            if (entity.HasVector3("scale"))
            {
                scale = entity.GetVector3("scale");
                entity.GetVector3Attribute("scale").ValueChanged += ScaleChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                rotation = entity.GetQuaternion("rotation");
                entity.GetQuaternionAttribute("rotation").ValueChanged += RotationChanged;
            }
            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            // load the model
            string meshName = entity.GetString("mesh");
            Model model = Game.Instance.Content.Load<Model>(meshName);

            string islandTextureName = entity.GetString("diffuse_texture");
            Texture2D islandTexture = Game.Instance.Content.Load<Texture2D>(islandTextureName);

            renderable = new IslandRenderable(scale, rotation, position, model);
            renderable.WindStrength = entity.GetFloat("wind_strength");
            renderable.DiffuseTexture = islandTexture;
            
            Game.Instance.Renderer.AddRenderable(renderable);
        }

        public void OnDetached(Entity entity)
        {
            Game.Instance.Renderer.RemoveRenderable(renderable);

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged -= PositionChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged -= RotationChanged;
            }
            if (entity.HasVector3("scale"))
            {
                entity.GetVector3Attribute("scale").ValueChanged -= ScaleChanged;
            }
        }

        public void Squash()
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new BoolRendererUpdate(renderable, "Squash", true));
        }

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Scale = newValue;
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            renderable.Rotation = newValue;
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Position = newValue;
        }

        private IslandRenderable renderable;
    }
}
