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
    public class IceSpikeRenderProperty : RendererUpdatableProperty
    {
        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            if (entity.HasVector3("velocity"))
            {
                entity.GetVector3Attribute("velocity").ValueChanged += VelocityChanged;
            }

            if (entity.HasBool("dead"))
            {
                entity.GetBoolAttribute("dead").ValueChanged += DeadChanged;
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
            if (entity.HasVector3("velocity"))
            {
                entity.GetVector3Attribute("velocity").ValueChanged -= VelocityChanged;
            }
            if (entity.HasBool("dead"))
            {
                entity.GetBoolAttribute("dead").ValueChanged -= DeadChanged;
            }

            base.OnDetached(entity);
        }

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            Vector3 position = Vector3.Zero;
            Vector3 velocity = Vector3.UnitZ;
            bool dead = false;

            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
            }

            if (entity.HasVector3("velocity"))
            {
                velocity = entity.GetVector3("velocity");
            }

            if (entity.HasBool("dead"))
            {
                dead = entity.GetBool("dead");
            }

            velocity.Normalize();

            return new IceSpikeRenderable(Game.Instance.Simulation.Time.At, 0, position, velocity, dead);
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

        private void VelocityChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            ChangeVector3("Direction", newValue);
        }

        private void DeadChanged(
            BoolAttribute sender,
            bool oldValue,
            bool newValue
        )
        {
            ChangeBool("Dead", newValue);
        }
    }
}
