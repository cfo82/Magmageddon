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
                pillar = DeterminePillar(entity);
                entity.AddStringAttribute("pillar", pillar.Name);
            }
            else
                pillar = Game.Instance.Simulation.EntityManager[entity.GetString("pillar")];
        }

        public override void OnDetached(Entity entity)
        {
            base.OnDetached(entity);
        }

        protected override void OnUpdate(Entity island, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            // get positions (ignore y component)
            Vector3 islandPos = island.GetVector3("position");
            Vector3 pillarPos = pillar.GetVector3("position");
            islandPos.Y = 0;
            pillarPos.Y = 0;

            // calculate radius
            Vector3 radiusV = islandPos - pillarPos;
 
            // rotate
            float delta = dt * constants.GetFloat("angle_speed");
            Matrix rotation = Matrix.CreateRotationY(delta);
            radiusV = Vector3.Transform(radiusV, rotation);

            // apply
            radiusV.Y = island.GetVector3("position").Y;
            island.SetVector3("position", pillarPos+radiusV);

            base.OnUpdate(island, gameTime);
        }

        protected override void CollisionHandler(GameTime gameTime, Entity entity, Contact co)
        {
            if (entity.HasString("kind") && (entity.GetString("kind") == "pillar"
                || entity.GetString("kind") == "pillar"))
            {
                // collision with pillar or island -> change direction
                dir = -dir;
            }
        }

        private Entity DeterminePillar(Entity island)
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
            return nearest;
        }

        private Entity pillar;
        private int dir = 1;
    }
}
