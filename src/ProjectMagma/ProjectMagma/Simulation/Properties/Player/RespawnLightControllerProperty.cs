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

        public override void OnAttached(AbstractEntity light)
        {
            this.light = light as Entity;
            this.island = Game.Instance.Simulation.EntityManager[light.GetString("island")];

            // register island change handler
            island.GetVector3Attribute(CommonNames.Position).ValueChanged += OnIslandPositionChanged;

            positionOffset = light.GetVector3(CommonNames.Position) - island.GetVector3(CommonNames.Position);

//            (light as Entity).Update += OnUpdate;
        }

        public override void OnDetached(
            AbstractEntity light
        )
        {
//            (light as Entity).Update -= OnUpdate;

            island.GetVector3Attribute(CommonNames.Position).ValueChanged -= OnIslandPositionChanged;
        }


        private void OnUpdate(Entity light, SimulationTime simTime)
        {
        }


        private void OnIslandPositionChanged(Vector3Attribute sender,
            Vector3 oldValue, Vector3 newValue)
        {
            light.SetVector3(CommonNames.Position, newValue + positionOffset);
        }

        private Vector3 positionOffset;
        private Entity island;
        private Entity light;
    }
}
