using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;


namespace ProjectMagma.Simulation
{
    public class IslandNoMovementControllerProperty : IslandControllerPropertyBase
    {
        public IslandNoMovementControllerProperty()
        {
        }

        public override void OnAttached(Entity entity)
        {
            base.OnAttached(entity);

            originalPositon = entity.GetVector3("position");

            if(entity.HasAttribute("fixed"))
                fixedPosition = entity.GetBool("fixed");
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);
        }

        public override void CalculateNewPosition(Entity island, ref Vector3 position, float dt)
        {
            // do nothing
        }

        protected override Vector3 GetNearestPointOnPath(ref Vector3 position)
        {
            if (fixedPosition)
                return originalPosition;
            else
                return position;
        }

        protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            // do nothing
        }


        private bool fixedPosition = true;
        private Vector3 originalPositon;
    }
}
