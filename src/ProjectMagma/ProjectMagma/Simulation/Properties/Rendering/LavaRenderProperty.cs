using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class LavaRenderProperty : RendererUpdatableProperty
    {
        public LavaRenderProperty()
        {
        }

        public override void OnAttached(Entity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3("scale"))
            {
                entity.GetVector3Attribute("scale").ValueChanged += ScaleChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged += RotationChanged;
            }
            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new AddRenderableUpdate((Renderable)Updatable));
        }

        public override void OnDetached(Entity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new RemoveRenderableUpdate((Renderable)Updatable));

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

            base.OnDetached(entity);
        }

        protected override ProjectMagma.Renderer.Interface.RendererUpdatable CreateUpdatable(Entity entity)
        {
            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            if (entity.HasVector3("scale"))
            {
                scale = entity.GetVector3("scale");
            }
            if (entity.HasQuaternion("rotation"))
            {
                rotation = entity.GetQuaternion("rotation");
            }
            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
            }

            // load the model
            string meshName = entity.GetString("mesh");
            Model model = Game.Instance.Content.Load<Model>(meshName);

            string sparseStuccoTextureName = entity.GetString("sparsestucco_texture");
            string fireFractalTextureName = entity.GetString("firefractal_texture");
            string vectorCloudTextureName = entity.GetString("vectorcloud_texture");
            string graniteTextureName = entity.GetString("granite_texture");

            Texture2D sparseStuccoTexture = Game.Instance.Content.Load<Texture2D>(sparseStuccoTextureName);
            Texture2D fireFractalTexture = Game.Instance.Content.Load<Texture2D>(fireFractalTextureName);
            Texture2D vectorCloudTexture = Game.Instance.Content.Load<Texture2D>(vectorCloudTextureName);
            Texture2D graniteTexture = Game.Instance.Content.Load<Texture2D>(graniteTextureName);

            return new LavaRenderable(scale, rotation, position, model, 
                sparseStuccoTexture, fireFractalTexture, vectorCloudTexture, graniteTexture);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
        }

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Scale", newValue);
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            ChangeQuaternion("Rotation", newValue);
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Position", newValue);
        }
    }
}
