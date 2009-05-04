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
            {
                pillar = Game.Instance.Simulation.EntityManager[entity.GetString("pillar")];

                // get radius
                Vector3 radiusV = (island.GetVector3("position") - pillar.GetVector3("position"));
                radiusV.Y = 0;
                radius = radiusV.Length();
            }
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

        protected override Vector3 GetNearestPointOnPath(ref Vector3 position)
        {
            // get direction of pillar
            Vector3 pillarPos = pillar.GetVector3("position");
            Vector3 dir = position - pillarPos;
            Debug.Assert(dir != Vector3.Zero);
            dir.Y = 0; // y is ignored
            if(dir != Vector3.Zero)
                dir.Normalize();
            Vector3 nearest = pillarPos + dir * radius; // get point on circular path
            nearest.Y = originalPosition.Y; // set height
            return nearest;
        }

        protected override void OnRepositioningEnded(Vector3 dir)
        {
            // set direction of circular motion so movements seems smooth
            Vector3 radiusDir = island.GetVector3("position") - pillar.GetVector3("position");
            this.dir = Math.Sign(Vector3.Dot(dir, radiusDir));
            if (this.dir == 0)
                this.dir = 1;
        }

        protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            {
                // change direction, if not already in direction away from that object

                // calculate direction of movement
                Vector3 oldPosition = island.GetVector3("position");
                Vector3 newPosition = oldPosition;
                CalculateNewPosition(island, ref newPosition, simTime.Dt);
                Vector3 diff = newPosition - oldPosition;

                // did we have collision last time too?
                if (hadCollision)
                {
                    Vector3 pushback = -normal * (island.GetVector3("position") - other.GetVector3("position")).Length();
                    pushback.Y = 0; // only in xz plane
                    // same direction already?
                    if (Vector3.Dot(diff, normal) > 0)
                    {
                        island.SetVector3("pushback_velocity", island.GetVector3("pushback_velocity") + pushback);
                    }
                    else
                    {
                        // change pushback dir
                        island.SetVector3("pushback_velocity", pushback);
                    }
                }
                else
                if (!(other.GetString("kind") == "island"
                     && other.GetString("attracted_by") != "")
                     && island.GetString("attracted_by") != null
                     && island.GetVector3("repulsion_velocity") == Vector3.Zero)
                {
                    // change dir
                    if (simTime.At > dirChangedAt + 1000) // todo: constant
                    {
                        dir = -dir;
                        dirChangedAt = simTime.At;
                    }

                    // check if normal is in opposite direction of theoretical velocity
                    if (Vector3.Dot(diff, normal) > 0)
                    {
                        //                    dir = -dir;

                        // always ensure we apply a bit of pushback out of other entity so we don't get stuck in there
                        if (!(other.GetString("kind") == "island"
                            && other.GetString("attracted_by") != "")
                            && island.GetString("attracted_by") != null
                            && island.GetVector3("repulsion_velocity") == Vector3.Zero)
                        {
                            Vector3 pushback = -normal * (island.GetVector3("position") - other.GetVector3("position")).Length();
                            pushback.Y = 0; // only in xz plane
                            island.SetVector3("pushback_velocity", island.GetVector3("pushback_velocity") + pushback);
                        }
                    }
                }
            }
        }

        private void AssignPillar(Entity island)
        {
            // find nearest pillar
            float dist = float.MaxValue;
            Entity nearest = null;
            foreach (Entity pillar in Game.Instance.Simulation.PillarManager)
            {
                Vector3 dir = pillar.GetVector3("position") - island.GetVector3("position");
                float d = dir.Length();
                if (d < dist)
                {
                    nearest = pillar;
                    dist = d;
                    dir.Y = 0;
                    radius = dir.Length();
                }
            }

            this.pillar = nearest;
            island.SetString("pillar", nearest.Name);
        }

        private float dirChangedAt = 0;
        private Entity pillar;
        private float radius;
        private int dir = 1;
    }
}
