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

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new AddRenderableUpdate((Renderable)Updatable));
        }

        public override void OnDetached(AbstractEntity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new RemoveRenderableUpdate((Renderable)Updatable));

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged -= PositionChanged;
            }

            base.OnDetached(entity);
        }

        protected override void SetUpdatableParameters(Entity entity)
        {
            
        }

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            Vector3 position = Vector3.Zero;

            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
            }

            return new RespawnLightRenderable(Game.Instance.Simulation.Time.At, position);
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
