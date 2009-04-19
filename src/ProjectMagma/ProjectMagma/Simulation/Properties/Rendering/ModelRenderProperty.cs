using System;
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
    public abstract class ModelRenderProperty : Property
    {
        public ModelRenderProperty()
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

            CreateRenderable(scale, rotation, position, model);
            SetRenderableParameters(entity);
            Game.Instance.Renderer.AddRenderable(Renderable);
        }

        public abstract void CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model);
        public abstract void SetRenderableParameters(Entity entity);

        public void OnDetached(Entity entity)
        {
            Game.Instance.Renderer.RemoveRenderable(Renderable);

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

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            Renderable.Scale = newValue;
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            Renderable.Rotation = newValue;
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            Renderable.Position = newValue;
        }

        protected ModelRenderable Renderable;
    }
}
