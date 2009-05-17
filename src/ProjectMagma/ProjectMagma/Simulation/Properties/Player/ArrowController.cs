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

        private void OnUpdate(Entity powerupEntity, SimulationTime simTime)
        {
            /*
            if (island != null)
            {
                Vector3 aimVector = island.GetVector3("position") + Vector3.UnitY * positionOffset.Y - player.GetVector3("position");

                // get intersection
                Ray3 ray = new Ray3(player.GetVector3("position"), aimVector);
                Vector3 arrowPos;
                if(Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, island, out arrowPos))
                    arrow.SetVector3("position", arrowPos);

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
                //arrow.SetQuaternion("rotation", Quaternion.Identity);
            }
             */
        }

        public void OnAttached(Entity arrow)
        {
            this.arrow = arrow;
            this.island = null;
            this.constants = Game.Instance.Simulation.EntityManager["player_constants"];

//            this.player = Game.Instance.Simulation.EntityManager[arrow.GetString("player")];

            // register island change handler
            arrow.GetStringAttribute("island").ValueChanged += OnIslandChanged;

            arrow.AddQuaternionAttribute("rotation", Quaternion.Identity);

            arrow.Update += OnUpdate;
        }

        public void OnDetached(
            Entity entity
        )
        {
            arrow.Update -= OnUpdate;

            arrow.GetStringAttribute("island").ValueChanged -= OnIslandChanged;

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
                positionOffset = island.GetVector3("landing_offset");

                // hack hackhack. should be in onAttached, but doesnt work there...
                this.player = Game.Instance.Simulation.EntityManager[arrow.GetString("player")];

            }
        }

        private void OnIslandPositionChanged(
            Vector3Attribute positionAttribute,
            Vector3 oldPosition,
            Vector3 newPosition
        )
        {
            Vector3 aimVector = newPosition /*+ Vector3.UnitY * positionOffset.Y*/ - player.GetVector3("position");

            // get intersection
            Ray3 ray = new Ray3(player.GetVector3("position"), aimVector);
            Vector3 arrowPos;
            if (Game.Instance.Simulation.CollisionManager.GetIntersectionPoint(ref ray, island, out arrowPos))
            {
                if(aimVector != Vector3.Zero)
                    aimVector.Normalize();
                arrow.SetVector3("position", arrowPos + -aimVector * constants.GetFloat("arrow_island_distance"));
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
        }

        private Vector3 positionOffset;
        private Entity arrow;
        private Entity player;
        private Entity island;
        private Entity constants;
    }
}
