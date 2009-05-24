using System;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Framework;

namespace ProjectMagma.Simulation
{
    public class RespawnLightControllerProperty : Property
    {

        public RespawnLightControllerProperty()
        {
        }

        public void OnAttached(AbstractEntity light)
        {
            this.light = light as Entity;
            this.island = Game.Instance.Simulation.EntityManager[light.GetString("island")];

            // register island change handler
            island.GetVector3Attribute("position").ValueChanged += OnIslandPositionChanged;

            positionOffset = light.GetVector3("position") - island.GetVector3("position");

//            (light as Entity).Update += OnUpdate;
        }

        public void OnDetached(
            AbstractEntity light
        )
        {
//            (light as Entity).Update -= OnUpdate;

            island.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;
        }


        private void OnUpdate(Entity light, SimulationTime simTime)
        {
        }


        private void OnIslandPositionChanged(Vector3Attribute sender,
            Vector3 oldValue, Vector3 newValue)
        {
            light.SetVector3("position", newValue + positionOffset);
        }

        private Vector3 positionOffset;
        private Entity island;
        private Entity light;
    }
}
