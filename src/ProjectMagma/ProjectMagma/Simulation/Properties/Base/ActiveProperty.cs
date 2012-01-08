using ProjectMagma.Framework;
using System.Collections.Generic;

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

        private struct Timeout
        {
            public bool repeat;
            public double timeoutAt;
            public ScheduledEventHandler handler;

            public Timeout(bool r, double t, ScheduledEventHandler h)
            {
                repeat = r;
                timeoutAt = t;
                handler = h;
            }
        }

        protected virtual void CheckScheduledEvents(Entity entity, SimulationTime simTime)
        {
            for (int i = timeouts.Count - 1; i >= 0; --i)
            {
                Timeout timeout = timeouts[i];
                if (timeout.timeoutAt <= simTime.At)
                {
                    timeout.handler.Invoke(entity, simTime);
                    timeouts.RemoveAt(i);
                }
            }
        }

        protected abstract void OnUpdate(Entity entity, SimulationTime simTime);
  
#region scheduling

        readonly List<Timeout> timeouts = new List<Timeout>();

        protected void scheduleOnce(double at, double delay, ScheduledEventHandler handler)
        {
            timeouts.Add(new Timeout(false, at + delay, handler));
        }

#endregion

    }
}
