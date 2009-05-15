﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class FlamethrowerRenderProperty : RendererUpdatableProperty
    {
        public override void OnAttached(Entity entity)
        {
            base.OnAttached(entity);

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged += PositionChanged;
            }

            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged += RotationChanged;
            }

            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new AddRenderableUpdate((Renderable)Updatable));
        }

        public override void OnDetached(Entity entity)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.AddUpdate(new RemoveRenderableUpdate((Renderable)Updatable));

            if (entity.HasVector3("position"))
            {
                entity.GetVector3Attribute("position").ValueChanged -= PositionChanged;
            }
            if (entity.HasQuaternion("rotation"))
            {
                entity.GetQuaternionAttribute("rotation").ValueChanged -= RotationChanged;
            }

            base.OnDetached(entity);
        }

        protected override RendererUpdatable CreateUpdatable(Entity entity)
        {
            Vector3 position = Vector3.Zero;
            Quaternion rotation = Quaternion.Identity;

            if (entity.HasVector3("position"))
            {
                position = entity.GetVector3("position");
            }

            if (entity.HasQuaternion("rotation"))
            {
                rotation = entity.GetQuaternion("rotation");
            }

            return new FlamethrowerRenderable(position, CalculateDirection(ref rotation));
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

        private Vector3 CalculateDirection(ref Quaternion rotation)
        {
            return Vector3.Transform(Vector3.UnitZ, Matrix.CreateFromQuaternion(rotation));
        }
    }
}