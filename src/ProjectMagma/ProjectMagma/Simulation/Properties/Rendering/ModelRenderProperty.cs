using System;
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
    public abstract class ModelRenderProperty : Property
    {
        public ModelRenderProperty()
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

            Renderable = CreateRenderable(scale, rotation, position, model);
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new AddRenderableUpdate(Renderable));
            SetRenderableParameters(entity);
        }

        public void OnDetached(Entity entity)
        {
            if (Game.Instance.Simulation.CurrentUpdateQueue != null)
            {
                Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new RemoveRenderableUpdate(Renderable));
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
        }

        protected abstract ModelRenderable CreateRenderable(Vector3 scale, Quaternion rotation, Vector3 position, Model model);
        protected abstract void SetRenderableParameters(Entity entity);

        #region Protected Change Utility Methods

        protected void ChangeBool(string id, bool value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new BoolRendererUpdate(Renderable, id, value));
        }

        protected void ChangeFloat(string id, float value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new FloatRendererUpdate(Renderable, id, value));
        }

        protected void ChangeInt(string id, int value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new IntRendererUpdate(Renderable, id, value));
        }

        protected void ChangeMatrix(string id, Matrix value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new MatrixRendererUpdate(Renderable, id, value));
        }

        protected void ChangeQuaternion(string id, Quaternion value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new QuaternionRendererUpdate(Renderable, id, value));
        }

        protected void ChangeString(string id, string value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new StringRendererUpdate(Renderable, id, value));
        }

        protected void ChangeVector2(string id, Vector2 value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new Vector2RendererUpdate(Renderable, id, value));
        }

        protected void ChangeVector3(string id, Vector3 value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new Vector3RendererUpdate(Renderable, id, value));
        }

        #endregion

        #region Private Change Listeners

        private void ScaleChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new Vector3RendererUpdate(Renderable, "Scale", newValue));
        }

        private void RotationChanged(
            QuaternionAttribute sender,
            Quaternion oldValue,
            Quaternion newValue
        )
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new QuaternionRendererUpdate(Renderable, "Rotation", newValue));
        }

        private void PositionChanged(
            Vector3Attribute sender,
            Vector3 oldValue,
            Vector3 newValue
        )
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new Vector3RendererUpdate(Renderable, "Position", newValue));
        }

        #endregion

        protected ModelRenderable Renderable;
    }
}
