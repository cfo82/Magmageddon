using System;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public class ArrowControllerProperty : Property
    {

        public ArrowControllerProperty()
        {
        }

        private void OnUpdate(Entity powerupEntity, SimulationTime simTime)
        {
        }

        public void OnAttached(Entity arrow)
        {
            this.arrow = arrow;
            this.island = null;

            // register island change handler
            arrow.GetStringAttribute("island").ValueChanged += OnIslandChanged;

            arrow.Update += OnUpdate;
        }

        public void OnDetached(
            Entity entity
        )
        {
            arrow.Update -= OnUpdate;

            if(island != null)
                island.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;
        }

        private void OnIslandChanged(StringAttribute sender,
            String oldIsland, String newIsland)
        {
            // reset handler on old island
            if(island != null)
                island.GetVector3Attribute("position").ValueChanged -= OnIslandPositionChanged;

            if ("" == newIsland)
            {
                island = null;
            }
            else
            {
                // register new island
                island = Game.Instance.Simulation.EntityManager[newIsland];
                island.GetVector3Attribute("position").ValueChanged += OnIslandPositionChanged; 
                positionOffset = island.GetVector3("landing_offset").Y;
            }
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            this.arrow.SetVector3("position", island.GetVector3("position") + Vector3.UnitY * positionOffset);
        }

        private float positionOffset = 0;
        private Entity arrow;
        private Entity island;
    }
}
