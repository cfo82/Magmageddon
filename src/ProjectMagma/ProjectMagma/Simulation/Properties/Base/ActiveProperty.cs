using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Shared.Math.Integration;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation
{
    public delegate void ScheduledEventHandler(Entity entity, SimulationTime at);

    public abstract class ActiveProperty: Property
    {
        ActivationStateChangedHandler activationDelegate;
        ActivationStateChangedHandler deactivationDelegate;

        public override void OnAttached(AbstractEntity entity)
        {
            this.activationDelegate = delegate(Property property) { RegisterUpdate(entity); };
            this.deactivationDelegate = delegate(Property property) { UnregisterUpdate(entity); };

            this.OnActivated += activationDelegate;
            this.OnDeactivated += deactivationDelegate;

            (entity as Entity).OnUpdate += CheckScheduledEvents;
        }

        public override void OnDetached(AbstractEntity entity)
        {
            this.OnActivated -= activationDelegate;
            this.OnDeactivated -= deactivationDelegate;

            (entity as Entity).OnUpdate -= CheckScheduledEvents;
        }

        private void RegisterUpdate(AbstractEntity entity)
        {
            (entity as Entity).OnUpdate += OnUpdate;
        }

        private void UnregisterUpdate(AbstractEntity entity)
        {
            (entity as Entity).OnUpdate -= OnUpdate;
        }

        protected virtual void CheckScheduledEvents(Entity entity, SimulationTime simTime)
        {
            int m = -1;
            foreach (KeyValuePair<double, ScheduledEventHandler> pair in timeouts)
            {
                if (pair.Key < simTime.At)
                    break;
                pair.Value.Invoke(entity, simTime);
                m++;
            }
            for (int i = m; i >= 0; i--)
            {
                timeouts.RemoveAt(i);
            }

        }

        protected abstract void OnUpdate(Entity entity, SimulationTime simTime);
  
#region scheduling

        readonly System.Collections.Generic.SortedList<double, ScheduledEventHandler> timeouts = new SortedList<double,ScheduledEventHandler>();

        private struct Timeout
        {
            bool repeat;
            double timeoutAt;
            ScheduledEventHandler handler;

            public Timeout(bool r, double t, ScheduledEventHandler h)
            {
                repeat = r;
                timeoutAt = t;
                handler = h;
            }
        }

        protected void scheduleOnce(double at, double delay, ScheduledEventHandler handler)
        {
            timeouts.Add(at + delay, handler);
        }

#endregion

    }
}
