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
    public abstract class PointExplosionRenderProperty : RendererUpdatableProperty
    {
        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3(CommonNames.Position))
            {
                entity.GetVector3Attribute(CommonNames.Position).ValueChanged += PositionChanged;
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

            base.OnDetached(entity);
        }

        protected abstract PointExplosionRenderable CreateExplosionRenderable(Entity entity, Vector3 position);

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            Vector3 position = Vector3.Zero;

            if (entity.HasVector3(CommonNames.Position))
            {
                position = entity.GetVector3(CommonNames.Position);
            }

            return CreateExplosionRenderable(entity, position);
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
    }
}
