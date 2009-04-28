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
    public class IceSpikeRenderProperty : Property
    {
        public void OnAttached(Entity entity)
        {
            Vector3 position = Vector3.Zero;
            Vector3 velocity = Vector3.UnitZ;

            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            if (entity.HasVector3("velocity"))
            {
                velocity = entity.GetVector3("velocity");
                entity.GetVector3Attribute("velocity").ValueChanged += VelocityChanged;
            }

            velocity.Normalize();

            renderable = new IceSpikeRenderable(position, velocity);

            Game.Instance.Renderer.AddRenderable(renderable);
        }

        public void OnDetached(Entity entity)
        {
            Game.Instance.Renderer.RemoveRenderable(renderable);

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged -= PositionChanged;
            }
            if (entity.HasQuaternion("velocity"))
            {
                entity.GetVector3Attribute("velocity").ValueChanged -= VelocityChanged;
            }
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Position = newValue;
        }

        private void VelocityChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            Vector3 direction = newValue;
            direction.Normalize();
            renderable.Direction = direction;
        }

        private IceSpikeRenderable renderable;
    }
}
