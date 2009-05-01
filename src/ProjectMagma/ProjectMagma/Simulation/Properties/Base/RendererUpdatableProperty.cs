using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public abstract class RendererUpdatableProperty : Property
    {
        public RendererUpdatableProperty()
        {
        }

        public virtual void OnAttached(Entity entity)
        {
            updatable = CreateUpdatable(entity);
            SetUpdatableParameters(entity);
        }

        public virtual void OnDetached(Entity entity)
        {
        }

        protected abstract RendererUpdatable CreateUpdatable(Entity entity);
        protected abstract void SetUpdatableParameters(Entity entity);

        #region Protected Change Utility Methods

        protected void ChangeBool(string id, bool value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new BoolRendererUpdate(updatable, id, value));
        }

        protected void ChangeFloat(string id, float value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new FloatRendererUpdate(updatable, id, value));
        }

        protected void ChangeInt(string id, int value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new IntRendererUpdate(updatable, id, value));
        }

        protected void ChangeMatrix(string id, Matrix value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new MatrixRendererUpdate(updatable, id, value));
        }

        protected void ChangeQuaternion(string id, Quaternion value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new QuaternionRendererUpdate(updatable, id, value));
        }

        protected void ChangeString(string id, string value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new StringRendererUpdate(updatable, id, value));
        }

        protected void ChangeVector2(string id, Vector2 value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new Vector2RendererUpdate(updatable, id, value));
        }

        protected void ChangeVector3(string id, Vector3 value)
        {
            Game.Instance.Simulation.CurrentUpdateQueue.updates.Add(new Vector3RendererUpdate(updatable, id, value));
        }

        #endregion

        protected RendererUpdatable Updatable
        {
            get { return updatable; }
        }

        private RendererUpdatable updatable;
    }
}