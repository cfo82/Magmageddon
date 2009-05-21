using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;


namespace ProjectMagma.Simulation
{
    public class IslandStraightMovementControllerProperty : IslandControllerPropertyBase
    {
        public IslandStraightMovementControllerProperty()
        {
        }

        public override void OnAttached(Entity entity)
        {
            base.OnAttached(entity);

            direction = entity.GetVector3("direction");
            Debug.Assert(direction != Vector3.Zero);
            direction.Normalize();
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);
        }

        public override Vector3 CalculateAccelerationDirection(Entity island, ref Vector3 position, ref Vector3 velocity, float acceleration, float dt)
        {
            Vector3 ppos = GetNearestPointOnPath(ref position);
            ppos += direction * acceleration * dt * dt;
            return ppos;
        }

        protected override Vector3 GetNearestPointOnPath(ref Vector3 position)
        {
            // project on direction
            float d = Vector3.Dot(direction, position - originalPosition);
            return originalPosition + direction * d;
        }

        protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            if (!HadCollision(simTime))
            {
                direction = -direction;
            }
        }

        private Vector3 direction;
    }
}
