using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class FlamethrowerRenderProperty : RendererUpdatableProperty
    {
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

            if (entity.HasBool(CommonNames.Fueled))
            {
                entity.GetBoolAttribute(CommonNames.Fueled).ValueChanged += FueledChanged;
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

            if (entity.HasBool(CommonNames.Fueled))
            {
                entity.GetBoolAttribute(CommonNames.Fueled).ValueChanged -= FueledChanged;
            }

            base.OnDetached(entity);
        }

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            Vector3 position = Vector3.Zero;
            Quaternion rotation = Quaternion.Identity;
            bool fueled = true;

            if (entity.HasVector3(CommonNames.Position))
            {
                position = entity.GetVector3(CommonNames.Position);
            }

            if (entity.HasQuaternion(CommonNames.Rotation))
            {
                rotation = entity.GetQuaternion(CommonNames.Rotation);
            }

            if (entity.HasBool(CommonNames.Fueled))
            {
                fueled = entity.GetBool(CommonNames.Fueled);
            }

            return new FlamethrowerRenderable(Game.Instance.Simulation.Time.At, 0, position, CalculateDirection(ref rotation), fueled);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Position", newValue);
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            ChangeVector3("Direction", CalculateDirection(ref newValue));
        }

        private void FueledChanged(
            BoolAttribute sender,
            bool oldValue,
            bool newValue
        )
        {
            ChangeBool("Fueled", newValue);
        }

        private Vector3 CalculateDirection(ref Quaternion rotation)
        {
            return Vector3.Transform(Vector3.UnitZ, Matrix.CreateFromQuaternion(rotation));
        }
    }
}
