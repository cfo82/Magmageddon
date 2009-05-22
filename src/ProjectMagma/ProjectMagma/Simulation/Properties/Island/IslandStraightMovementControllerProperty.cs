﻿using System;
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

        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            direction = entity.GetVector3("direction");
            Debug.Assert(direction != Vector3.Zero);
            direction.Normalize();
        }

        public override void OnDetached(AbstractEntity entity)
        {
            base.OnDetached(entity);
        }

        public override Vector3 CalculateAccelerationDirection(Entity island, ref Vector3 position, ref Vector3 velocity, float acceleration, float dt)
        {
            Vector3 ppos = GetNearestPointOnPath(ref position);
            Vector3 newpos = ppos + direction * acceleration * dt * dt;
            Vector3 dir = newpos - ppos;
            if (dir != Vector3.Zero)
                dir.Normalize();
            return dir;
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
