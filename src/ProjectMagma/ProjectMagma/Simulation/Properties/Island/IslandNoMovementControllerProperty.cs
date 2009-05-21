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
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);
        }

        public override Vector3 CalculateAccelerationDirection(Entity island, ref Vector3 position, ref Vector3 velocity, float acceleration, float dt)
        {
            return Vector3.Zero;
        }

        protected override Vector3 GetNearestPointOnPath(ref Vector3 position)
        {
            return originalPosition;
        }

        protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            // do nothing
        }

    }
}
