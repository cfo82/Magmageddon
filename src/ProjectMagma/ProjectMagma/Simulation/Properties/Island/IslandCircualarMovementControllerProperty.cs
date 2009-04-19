using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;


namespace ProjectMagma.Simulation
{
    public class IslandCircualarMovementControllerProperty : IslandControllerPropertyBase
    {
        public IslandCircualarMovementControllerProperty()
        {
        }

        public override void OnAttached(Entity entity)
        {
            base.OnAttached(entity);

            if (!entity.HasAttribute("pillar"))
            {
                entity.AddStringAttribute("pillar", "");
                AssignPillar(entity);
            }
            else
                pillar = Game.Instance.Simulation.EntityManager[entity.GetString("pillar")];
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);
        }

        public override void CalculateNewPosition(Entity island, ref Vector3 position, float dt)
        {
            // get positions (ignore y component)
            Vector3 islandPos = position;
            Vector3 pillarPos = pillar.GetVector3("position");
            islandPos.Y = 0;
            pillarPos.Y = 0;

            // calculate radius
            Vector3 radiusV = islandPos - pillarPos;
 
            // rotate
            float delta = dir * dt * constants.GetFloat("angle_speed");
            Matrix rotation = Matrix.CreateRotationY(delta);
            radiusV = Vector3.Transform(radiusV, rotation);

            // set Y
            position.X = pillarPos.X + radiusV.X;
            position.Z = pillarPos.Z + radiusV.Z;
        }

        protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co)
        {
            if ((other.GetString("kind") == "pillar"
                || other.GetString("kind") == "island"))
            {
                // collision with pillar or island -> change direction, 
                // if not already in direction away from that object
                
                // calculate direction of movement
                Vector3 oldPosition = island.GetVector3("position");
                Vector3 newPosition = oldPosition;
                CalculateNewPosition(island, ref newPosition, simTime.Dt);
                Vector3 diff = newPosition - oldPosition;
                // check if normal is in opposite direction of theoretical velocity
                if (Vector3.Dot(diff, co[0].Normal) > 0)
                {
                    dir = -dir;
                }
            }
        }


        protected override void OnRepulsionEnd()
        {
//            AssignPillar(island);
            base.OnRepulsionEnd();
        }

        protected override void OnAttractionEnd()
        {
//            AssignPillar(island);
            base.OnAttractionEnd();
        }

        private void AssignPillar(Entity island)
        {
            // find nearest pillar
            float dist = float.MaxValue;
            Entity nearest = null;
            foreach (Entity pillar in Game.Instance.Simulation.PillarManager)
            {
                float d = (pillar.GetVector3("position") - island.GetVector3("position")).Length();
                if (d < dist)
                {
                    nearest = pillar;
                    dist = d;
                }
            }

            this.pillar = nearest;
            island.SetString("pillar", nearest.Name);
        }

        private Entity pillar;
        private int dir = 1;
    }
}
