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
    public class RenderHighlightProperty : Property
    {
        public RenderHighlightProperty()
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

            enabled = false;
            renderable = new HighlightRenderable(scale, rotation, position, model);
            this.entity = entity;

            // attach listener for management form
            #if !XBOX
            Game.Instance.ManagementForm.EntitySelectionChanged += OnEntitySelectionChanged;
            #endif
        }

        public void OnDetached(Entity entity)
        {
            #if !XBOX
            Game.Instance.ManagementForm.EntitySelectionChanged -= OnEntitySelectionChanged;
            #endif
            if (enabled)
            {
                Game.Instance.Renderer.RemoveRenderable(renderable);
            }

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

            entity = null;
        }

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Scale = newValue;
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            renderable.Rotation = newValue;
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            renderable.Position = newValue;
        }

#if !XBOX
        private void OnEntitySelectionChanged(ManagementForm managementForm, Entity oldSelection, Entity newSelection)
        {
            if (enabled)
            {
                Game.Instance.Renderer.RemoveRenderable(renderable);
            }

            enabled = this.entity == newSelection;

            if (enabled)
            {
                Game.Instance.Renderer.AddRenderable(renderable);
            }
        }
#endif

        private HighlightRenderable renderable;
        private bool enabled;
        private Entity entity;
    }
}
