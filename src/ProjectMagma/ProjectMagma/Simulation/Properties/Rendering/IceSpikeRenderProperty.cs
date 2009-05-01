using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer.Interface;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class IceSpikeRenderProperty : Property
    {
        public void OnAttached(Entity entity)
        {
            Vector3 position = Vector3.Zero;
            Vector3 velocity = Vector3.UnitZ;
            bool dead = false;

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

            if (entity.HasBool("dead"))
            {
                dead = entity.GetBool("dead");
                entity.GetBoolAttribute("dead").ValueChanged += DeadChanged;
            }

            velocity.Normalize();

            renderable = new IceSpikeRenderable(position, velocity, dead);
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new AddRenderableUpdate(renderable));
        }

        public void OnDetached(Entity entity)
        {
            if (Game.Instance.Simulation.CurrentUpdateQueue != null)
            {
                Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new RemoveRenderableUpdate(renderable));
            }

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
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(
                new Vector3RendererUpdate(renderable, "Position", newValue)
            );
        }

        private void VelocityChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(
                new Vector3RendererUpdate(renderable, "Direction", newValue)
            );
        }

        private void DeadChanged(
            BoolAttribute sender,
            bool oldValue,
            bool newValue
        )
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(
                new BoolRendererUpdate(renderable, "Dead", newValue)
            );
        }

        private IceSpikeRenderable renderable;
    }
}
