using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            entity.Update += new UpdateHandler(OnUpdate);
            entity.AddIntAttribute("collisionCount", 0);
        }

        public void OnDetached(Entity entity)
        {
            entity.Update -= new UpdateHandler(OnUpdate);
            // TODO: remove attribute!
        }

        private void OnUpdate(Entity entity, GameTime gameTime)
        {
            float dt = ((float)gameTime.ElapsedGameTime.Milliseconds)/1000.0f;

            // control this island
            // read out attributes
            Vector3 v = entity.GetVector3("velocity");

            // first force contribution: random               
            Vector3 a = new Vector3(
                (float)rand.NextDouble() - 0.5f,
                0.0f,
                (float)rand.NextDouble() - 0.5f
            ) * islandRandomStrength;


            // second force contribution: collision with pillars
            Vector3 islandPosition = entity.GetVector3("position");
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
                    Debug.Assert(scale.X == scale.Y && scale.Y == scale.Z);
                    pillarScale *= scale.X;
                }

                BoundingBox islandBox = (BoundingBox)Game.Instance.Content.Load<Model>("Models/island_primitive").Tag;
                float islandScale = islandBox.Max.X;
                if (entity.HasVector3("scale"))
                {
                    Vector3 scale = entity.GetVector3("scale");
                    Debug.Assert(scale.X == scale.Y && scale.Y == scale.Z);
                    islandScale *= scale.X;
                }

                // collision detection
                if (dist.Length() > pillarScale + islandScale)
                {
                    // no collision with this pillar
                    pillarContribution = dist;
                    pillarContribution *= pillarContribution.Length() * pillarAttraction;
                }
                else
                {
                    // island collided with this pillar
                    pillarContribution = -dist * pillarRepulsion;// *(pillarIslandCollisionRadius - dist.Length()) * 10.0f;
                    if (entity.GetInt("collisionCount") == 0)
                    {
                        // perform elastic collision if its the first time

                        v = -v * (1.0f - pillarElasticity);
                        //Console.WriteLine("switching dir " + (e.Attributes["collisionCount"] as IntAttribute).Value);// + " " + rand.NextDouble());
                    }
                    else
                    {
                        // in this case, the island is stuck. try gradually increasing
                        // the opposing force until the island manages to escape.

                        pillarContribution *= entity.GetInt("collisionCount");
                        //Console.WriteLine("contrib " + pillarContribution);
                    }
                    collided = true;
                }
                a += pillarContribution;
            }

            if (!collided)
            {
                entity.SetInt("collisionCount", 0);
            }
            else
            {
                entity.SetInt("collisionCount", entity.GetInt("collisionCount") + 1);
            }

            // compute final acceleration
            entity.SetVector3("acceleration", a);

            // compute final velocity
            v = (v + dt * entity.GetVector3("acceleration")) * (1.0f - islandDamping);
            float velocityLength = v.Length();
            if (velocityLength > islandMaxVelocity)
            {
                v *= islandMaxVelocity / velocityLength;
            }
            entity.SetVector3("velocity", v);

            // compute final position
            entity.SetVector3("position", entity.GetVector3("position") + dt * entity.GetVector3("velocity"));
        }

        private Random rand;
        private static float islandRandomStrength = 1000.0f;
        private static float islandMaxVelocity = 200;
        private static float pillarElasticity = 0.1f;
        private static float pillarAttraction = 0.0005f;
        private static float pillarRepulsion = 0.03f;
        private static float islandDamping = 0.001f;
    }
}
