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

            Debug.Assert(entity.HasVector3("velocity"));
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);
        }

        public override void CalculateNewPosition(Entity island, ref Vector3 position, float dt)
        {
            position += island.GetVector3("velocity") * dt;
        }

        protected override Vector3 GetNearestPointOnPath(ref Vector3 position)
        {
            // todo: calculate postion on path (can be done with originalpositon and velocity vector)
            return position;
        }

        protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            island.SetVector3("velocity", -island.GetVector3("velocity"));
        }

    }
}
