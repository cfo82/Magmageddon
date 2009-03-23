using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMagma.Collision;

namespace ProjectMagma.Framework
{
    public class IslandControllerProperty : Property
    {
        public IslandControllerProperty()
        {
            rand = new Random(485394);
        }

        public void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasVector3("position"));

            entity.AddIntAttribute("collisionCount", 0);

            // TODO: make this an attribute or add a constant for specifing health/size multiplier
            entity.AddIntAttribute("health", (int)entity.GetVector3("scale").Length() * 2);

            this.constants = Game.Instance.EntityManager["island_constants"];

            entity.Update += OnUpdate;

            ((CollisionProperty)entity.GetProperty("collision")).OnContact += new ContactHandler(CollisionHandler);

            originalPosition = entity.GetVector3("position");
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= OnUpdate;
            if (entity.HasProperty("collision"))
            {
                ((CollisionProperty)entity.GetProperty("collision")).OnContact -= new ContactHandler(CollisionHandler);
            }
            // TODO: remove attribute!
        }

        private void OnUpdate(Entity island, GameTime gameTime)
        {
            if (island.GetInt("health") <= 0)
            {
                Game.Instance.EntityManager.RemoveDeferred(island);
                return;
            }

            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            // control this island
            // read out attributes
            Vector3 v = island.GetVector3("velocity");

            // first force contribution: random               
            Vector3 a = new Vector3(
                (float)rand.NextDouble() - 0.5f,
                0.0f,
                (float)rand.NextDouble() - 0.5f
            ) * constants.GetFloat("random_strength");


            // second force contribution: collision with pillars
            Vector3 islandPosition = island.GetVector3("position");
            bool collided = false;

            foreach (Entity pillar in Game.Instance.PillarManager)
            {
                Vector3 pillarPosition = pillar.GetVector3("position");
                Vector3 dist = pillarPosition - islandPosition;
                dist.Y = 0;
                Vector3 pillarContribution;

                BoundingBox pillarBox = (BoundingBox)Game.Instance.Content.Load<Model>("Models/pillar_primitive").Tag;
                float pillarScale = pillarBox.Max.X;
                if (pillar.HasVector3("scale"))
                {
                    Vector3 scale = pillar.GetVector3("scale");
                    Debug.Assert(scale.X == scale.Z);
                    pillarScale *= scale.X;
                }

                BoundingBox islandBox = (BoundingBox)Game.Instance.Content.Load<Model>("Models/island_primitive").Tag;
                float islandScale = islandBox.Max.X;
                if (island.HasVector3("scale"))
                {
                    Vector3 scale = island.GetVector3("scale");
                    Debug.Assert(scale.X == scale.Z);
                    islandScale *= scale.X;
                }

                // collision detection
                if (dist.Length() > pillarScale + islandScale)
                {
                    // no collision with this pillar
                    pillarContribution = dist;
                    pillarContribution *= pillarContribution.Length() * constants.GetFloat("pillar_attraction");
                }
                else
                {
                    // island collided with this pillar
                    pillarContribution = -dist * constants.GetFloat("pillar_repulsion");// *(pillarIslandCollisionRadius - dist.Length()) * 10.0f;
                    if (island.GetInt("collisionCount") == 0)
                    {
                        // perform elastic collision if its the first time

                        v = -v * (1.0f - constants.GetFloat("pillar_elasticity"));
                        //Console.WriteLine("switching dir " + (e.Attributes["collisionCount"] as IntAttribute).Value);// + " " + rand.NextDouble());
                    }
                    else
                    {
                        // in this case, the island is stuck. try gradually increasing
                        // the opposing force until the island manages to escape.

                        pillarContribution *= island.GetInt("collisionCount");
                        //Console.WriteLine("contrib " + pillarContribution);
                    }
                    collided = true;
                }
                a += pillarContribution;
            }

            if (!collided)
            {
                island.SetInt("collisionCount", 0);
            }
            else
            {
                island.SetInt("collisionCount", island.GetInt("collisionCount") + 1);
            }

            // compute final acceleration
            island.SetVector3("acceleration", a);

            // compute final velocity
            v = (v + dt * island.GetVector3("acceleration")) * (1.0f - constants.GetFloat("damping"));
            float velocityLength = v.Length();
            if (velocityLength > constants.GetFloat("max_velocity"))
            {
                v *= constants.GetFloat("max_velocity") / velocityLength;
            }
            island.SetVector3("velocity", v);

            // compute final position
            island.SetVector3("position", island.GetVector3("position") + dt * island.GetVector3("velocity"));

            // implement sinking/rising islands...
            Vector3 position = island.GetVector3("position");
            if (playersOnIsland > 0)
            {
                position += dt * constants.GetFloat("sinking_speed") * playersOnIsland * (-Vector3.UnitY);
                playerLeftAt = 0;
            }
            else
            {
                if (position.Y < originalPosition.Y && gameTime.TotalGameTime.TotalMilliseconds > playerLeftAt + constants.GetInt("rising_delay"))
                {
                    if (playerLeftAt == 0)
                        playerLeftAt = gameTime.TotalGameTime.TotalMilliseconds;
                    position += dt * constants.GetFloat("rising_speed") * Vector3.UnitY;
                }
            }

            if (position.Y > originalPosition.Y)
            {
                position.Y = originalPosition.Y;
            }
            island.SetVector3("position", position);

            playersOnIsland = 0;
        }

        private void CollisionHandler(GameTime gameTime, Contact contact)
        {
            Entity player = contact.entityB;
            if (player.HasString("kind") && // other entity has a kind-attribute
                player.GetString("kind") == "player" && // other entity is a player
                contact.normal.Y > 0 // player is above island
            )
            {
                playersOnIsland++;
            }
        }

        private Entity constants;
        private Random rand;
        private int playersOnIsland;
        private double playerLeftAt;
        private Vector3 originalPosition;
    }
}
