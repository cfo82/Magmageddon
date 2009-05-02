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

// todo: this should inherit from basicrenderproperty
namespace ProjectMagma.Simulation
{
    public class IslandRenderProperty : RendererUpdatableProperty
    {
        public IslandRenderProperty()
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

        protected override RendererUpdatable CreateUpdatable(Entity entity)
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
            Model model = Game.Instance.ContentManager.Load<Model>(meshName);

            string islandTextureName = entity.GetString("diffuse_texture");
            Texture2D islandTexture = Game.Instance.ContentManager.Load<Texture2D>(islandTextureName);

            return new IslandRenderable(scale, rotation, position, model, islandTexture);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            ChangeFloat("WindStrength", entity.GetFloat("wind_strength"));
        }

        public void Squash()
        {
            ChangeBool("Squash", true);
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
