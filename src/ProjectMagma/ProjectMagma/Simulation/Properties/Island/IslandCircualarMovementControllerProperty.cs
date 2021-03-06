﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision;
using ProjectMagma.Framework;


namespace ProjectMagma.Simulation
{
    public class IslandCircualarMovementControllerProperty : IslandControllerPropertyBase
    {
        public IslandCircualarMovementControllerProperty()
        {
        }

        public override void OnAttached(AbstractEntity entity)
        {
            base.OnAttached(entity);

            if (!entity.HasAttribute("pillar"))
            {
                entity.AddStringAttribute("pillar", "");
                AssignPillar(entity as Entity);
            }
            else
            {
                pillar = Game.Instance.Simulation.EntityManager[entity.GetString("pillar")];

                // get radius
                Vector3 radiusV = (island.GetVector3(CommonNames.Position) - pillar.GetVector3(CommonNames.Position));
                radiusV.Y = 0;
                radius = radiusV.Length();
            }
        }

        public override void OnDetached(AbstractEntity entity)
        {
            base.OnDetached(entity);
        }

        public override Vector3 CalculateAccelerationDirection(Entity island, ref Vector3 position, ref Vector3 velocity, float acceleration, float dt)
        {
            // get positions (ignore y component)
            Vector3 islandPos = position;
            Vector3 pillarPos = pillar.GetVector3(CommonNames.Position);
            islandPos.Y = 0;
            pillarPos.Y = 0;

            // calculate radius
            Vector3 radiusV = islandPos - pillarPos;

            if (radiusV != Vector3.Zero)
            {
                // get normal radius vector
                Vector3 radiusN = Vector3.Normalize(radiusV);
                Vector3 diff = radiusN * radius - radiusV; // difference from desired to current radius
                float rd = diff.Length();
                if (diff != Vector3.Zero)
                    diff.Normalize();
                diff *= rd / radius * acceleration;

//                Console.WriteLine("diff: " + diff);

                // get acceleration direction
                Vector3 adir = Vector3.Cross(radiusN, Vector3.UnitY * dir);

                // calculare final direction
                Vector3 fdir = adir * acceleration + diff;
                if (fdir != Vector3.Zero)
                    fdir.Normalize();

//                Console.WriteLine("fdir: " + fdir);

                return fdir;
            }
            else
                return Vector3.Zero;
        }

        protected override Vector3 GetNearestPointOnPath(ref Vector3 position)
        {
            // get direction of pillar
            Vector3 pillarPos = pillar.GetVector3(CommonNames.Position);
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
            Vector3 radiusDir = island.GetVector3(CommonNames.Position) - pillar.GetVector3(CommonNames.Position);
            this.dir = Math.Sign(Vector3.Dot(dir, radiusDir));
            if (this.dir == 0)
                this.dir = 1;
        }

        protected override bool HandleCollision(SimulationTime simTime, Entity island, Entity other, Contact co, ref Vector3 normal)
        {
            // change dir
            if (other.HasAttribute(CommonNames.Kind)
                && other.GetString(CommonNames.Kind) != "island" // we don't change direction for other islands
                && other.GetString(CommonNames.Kind) != "player" // or players
                && simTime.At > dirChangedAt + 1000) // todo: extract constant
            {
                dir = -dir;
                dirChangedAt = simTime.At;

                // collision handled
                return true;
            }
            else
                return false;
        }

        private void AssignPillar(Entity island)
        {
            // find nearest pillar
            float dist = float.MaxValue;
            Entity nearest = null;
            foreach (Entity pillar in Game.Instance.Simulation.PillarManager)
            {
                Vector3 dir = pillar.GetVector3(CommonNames.Position) - island.GetVector3(CommonNames.Position);
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
