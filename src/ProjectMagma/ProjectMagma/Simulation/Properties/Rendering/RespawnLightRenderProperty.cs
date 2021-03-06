﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Renderables;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class RespawnLightRenderProperty : RendererUpdatableProperty
    {
        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3(CommonNames.Position))
            {
                entity.GetVector3Attribute(CommonNames.Position).ValueChanged += PositionChanged;
            }

            if (entity.HasBool(CommonNames.Hide))
            {
                entity.GetBoolAttribute(CommonNames.Hide).ValueChanged += HideChanged;
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

            if (entity.HasBool(CommonNames.Hide))
            {
                entity.GetBoolAttribute(CommonNames.Hide).ValueChanged -= HideChanged;
            }

            base.OnDetached(entity);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            
        }

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            int renderPriority = 0;
            Vector3 position = Vector3.Zero;

            if (entity.HasInt(CommonNames.RenderPriority))
            {
                renderPriority = entity.GetInt(CommonNames.RenderPriority);
            }
            if (entity.HasVector3(CommonNames.Position))
            {
                position = entity.GetVector3(CommonNames.Position);
            }

            return new RespawnLightRenderable(Game.Instance.Simulation.Time.At, renderPriority, position - new Vector3(0, 25, 10));
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Position", newValue - new Vector3(0, 25, 10));
        }

        private void HideChanged(
            BoolAttribute sender,
            bool oldValue,
            bool newValue
        )
        {
            ChangeBool("Hide", newValue);
        }
    }
}
