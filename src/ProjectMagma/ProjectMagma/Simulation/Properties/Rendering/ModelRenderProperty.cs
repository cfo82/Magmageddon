using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Model;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;


namespace ProjectMagma.Simulation
{
    public abstract class ModelRenderProperty : RendererUpdatableProperty
    {
        public ModelRenderProperty()
        {
        }

        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3(CommonNames.Position))
            {
                entity.GetVector3Attribute(CommonNames.Position).ValueChanged += PositionChanged;
            }
            if (entity.HasQuaternion(CommonNames.Rotation))
            {
                entity.GetQuaternionAttribute(CommonNames.Rotation).ValueChanged += RotationChanged;
            }
            if (entity.HasVector3(CommonNames.Scale))
            {
                entity.GetVector3Attribute(CommonNames.Scale).ValueChanged += ScaleChanged;
            }

            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new AddRenderableUpdate((Renderable)Updatable));
        }

        public override void OnDetached(AbstractEntity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new RemoveRenderableUpdate((Renderable)Updatable));

            if (entity.HasVector3(CommonNames.Position))
            {
                entity.GetVector3Attribute(CommonNames.Position).ValueChanged -= PositionChanged;
            }
            if (entity.HasQuaternion(CommonNames.Rotation))
            {
                entity.GetQuaternionAttribute(CommonNames.Rotation).ValueChanged -= RotationChanged;
            }
            if (entity.HasVector3(CommonNames.Scale))
            {
                entity.GetVector3Attribute(CommonNames.Scale).ValueChanged -= ScaleChanged;
            }

            base.OnDetached(entity);
        }

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            int renderPriority = 0;
            Vector3 scale = Vector3.One;
            Quaternion rotation = Quaternion.Identity;
            Vector3 position = Vector3.Zero;

            if (entity.HasInt(CommonNames.RenderPriority))
            {
                renderPriority = entity.GetInt(CommonNames.RenderPriority);
            }
            if (entity.HasVector3(CommonNames.Scale))
            {
                scale = entity.GetVector3(CommonNames.Scale);
            }
            if (entity.HasQuaternion(CommonNames.Rotation))
            {
                rotation = entity.GetQuaternion(CommonNames.Rotation);
            }
            if (entity.HasVector3(CommonNames.Position))
            {
                position = entity.GetVector3(CommonNames.Position);
            }

            // load the model
            if (!entity.HasString(CommonNames.Mesh))
                { throw new Exception(string.Format("missing 'mesh' attribute on entity '{0}'", entity.Name)); }
            string meshName = entity.GetString(CommonNames.Mesh);
            Model model = Game.Instance.ContentManager.Load<MagmaModel>(meshName).XnaModel;

            return CreateRenderable(entity, renderPriority, scale, rotation, position, model);
        }

        protected abstract ModelRenderable CreateRenderable(Entity entity, int renderPriority, Vector3 scale, Quaternion rotation, Vector3 position, Model model);

        #region Private Change Listeners

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

        #endregion
    }
}
