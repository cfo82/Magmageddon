using System;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation
{
    public class ArrowControllerProperty : Property
    {

        public ArrowControllerProperty()
        {
        }

        private float relPos;

        public void OnAttached(AbstractEntity arrow)
        {
            this.arrow = arrow as Entity;
            this.island = null;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

//            this.player = Game.Instance.Simulation.EntityManager[arrow.GetString("player")];

            // register island change handler
            arrow.GetStringAttribute("island").ValueChanged += OnIslandChanged;

            arrow.AddQuaternionAttribute("rotation", Quaternion.Identity);

            relPos = constants.GetFloat("arrow_island_min_distance_factor");

            (arrow as Entity).Update += OnUpdate;
        }

        public void OnDetached(
            AbstractEntity entity
        )
        {
            arrow.Update -= OnUpdate;

            arrow.GetStringAttribute("island").ValueChanged -= OnIslandChanged;
        }


        private void OnUpdate(Entity powerupEntity, SimulationTime simTime)
        {
            if (island != null)
            {
                Vector3 playerPos = player.GetVector3("position");
                Vector3 aimVector = island.GetVector3("position") - playerPos;

                // get intersection
                Ray3 ray = new Ray3(playerPos, aimVector);
                Vector3 arrowPos;
                if (Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, island, out arrowPos))
                {
                    Vector3 delta = (arrowPos - playerPos) * relPos;
                    arrow.SetVector3("position", playerPos + delta);
                }

                // point arrow from player
                Vector3 tminusp = -aimVector;
                Vector3 ominusp = Vector3.Up;
                if (tminusp != Vector3.Zero)
                    tminusp.Normalize();
                float theta = (float)System.Math.Acos(Vector3.Dot(tminusp, ominusp));
                Vector3 cross = Vector3.Cross(ominusp, tminusp);

                if (cross != Vector3.Zero)
                    cross.Normalize();

                Quaternion targetQ = Quaternion.CreateFromAxisAngle(cross, theta);

                arrow.SetQuaternion("rotation", targetQ);

                relPos += simTime.Dt * constants.GetFloat("arrows_per_second");

                if (relPos > constants.GetFloat("arrow_island_max_distance_factor"))
                {
                    relPos = constants.GetFloat("arrow_island_min_distance_factor");
                }
            }
        }


        private void OnIslandChanged(StringAttribute sender,
            String oldIsland, String newIsland)
        {
            if ("" == newIsland)
            {
                island = null;
            }
            else
            {
                // register new island
                island = Game.Instance.Simulation.EntityManager[newIsland];
                positionOffset = island.GetVector3("landing_offset");

                // hack hackhack. should be in onAttached, but doesnt work there...
                this.player = Game.Instance.Simulation.EntityManager[arrow.GetString("player")];

                relPos = 0;
            }
        }

 
        private Vector3 positionOffset;
        private Entity arrow;
        private Entity player;
        private Entity island;
        private Entity constants;
    }
}
