using System;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public class ExplosionControllerProperty : Property
    {

        public ExplosionControllerProperty()
        {
        }

        private void OnUpdate(Entity explosion, SimulationTime simTime)
        {
            if(simTime.At > liveTo)
            {
                Game.Instance.Simulation.EntityManager.RemoveDeferred(explosion);
            }
        }

        public void OnAttached(Entity explosion)
        {
            liveTo = Game.Instance.Simulation.Time.At + explosion.GetInt("live_span");
            explosion.Update += OnUpdate;
        }

        public void OnDetached(
            Entity explosion
        )
        {
            explosion.Update -= OnUpdate;
        }

        private float liveTo;
    }
}
