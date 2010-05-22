using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Model;
using ProjectMagma.Simulation.Collision;


namespace ProjectMagma.Simulation
{

    //public class IslandPhysicalMovementControllerProperty : IslandControllerPropertyBase
    //{
    //    public IslandPhysicalMovementControllerProperty()
    //    {
    //        rand = new Random(485394);
    //    }

    //    public override void OnAttached(Entity entity)
    //    {
    //        base.OnAttached(entity);

    //        entity.AddIntAttribute("collisionCount", 0);
    //    }

    //    public override void OnDetached(Entity entity)
    //    {
    //        base.OnDetached(entity);
    //    }

    //    protected override void OnUpdate(Entity island, SimulationTime simTime)
    //    {
    //        float dt = simTime.Dt;

    //        // control this island
    //        // read out attributes
    //        Vector3 v = island.GetVector3(CommonNames.Velocity);

    //        // first force contribution: random               
    //        Vector3 a = new Vector3(
    //            (float)rand.NextDouble() - 0.5f,
    //            0.0f,
    //            (float)rand.NextDouble() - 0.5f
    //        ) * constants.GetFloat("random_strength");


    //        // second force contribution: collision with pillars
    //        Vector3 islandPosition = island.GetVector3(CommonNames.Position);
    //        bool collided = false;

    //        foreach (Entity pillar in Game.Instance.Simulation.PillarManager)
    //        {
    //            Vector3 pillarPosition = pillar.GetVector3(CommonNames.Position);
    //            Vector3 dist = pillarPosition - islandPosition;
    //            dist.Y = 0;
    //            Vector3 pillarContribution;

    //            /*BoundingBox pillarBox = (BoundingBox)Game.Instance.ContentManager.Load<Model>("Models/pillar_primitive").Tag;
    //            float pillarScale = pillarBox.Max.X;
    //            if (pillar.HasVector3(CommonNames.Scale))
    //            {
    //                Vector3 scale = pillar.GetVector3(CommonNames.Scale);
    //                Debug.Assert(scale.X == scale.Z);
    //                pillarScale *= scale.X;
    //            }*/

    //            /*BoundingBox islandBox = (BoundingBox)Game.Instance.ContentManager.Load<Model>("Models/island_primitive").Tag;
    //            float islandScale = islandBox.Max.X;
    //            if (island.HasVector3(CommonNames.Scale))
    //            {
    //                Vector3 scale = island.GetVector3(CommonNames.Scale);
    //                Debug.Assert(scale.X == scale.Z);
    //                islandScale *= scale.X;
    //            }*/

    //            // collision detection with pillars
    //            /*if (dist.Length() > pillarScale + islandScale)
    //            {*/
    //                // no collision with this pillar
    //                pillarContribution = dist;
    //                pillarContribution *= pillarContribution.Length() * constants.GetFloat("pillar_attraction");
    //            /*}
    //            else
    //            {
    //                // island collided with this pillar
    //                pillarContribution = -dist * constants.GetFloat("pillar_repulsion");// *(pillarIslandCollisionRadius - dist.Length()) * 10.0f;
    //                if (island.GetInt("collisionCount") == 0)
    //                {
    //                    // perform elastic collision if its the first time

    //                    v = -v * (1.0f - constants.GetFloat("pillar_elasticity"));
    //                    //Console.WriteLine("switching dir " + (e.Attributes["collisionCount"] as IntAttribute).Value);// + " " + rand.NextDouble());
    //                }
    //                else
    //                {
    //                    // in this case, the island is stuck. try gradually increasing
    //                    // the opposing force until the island manages to escape.

    //                    pillarContribution *= island.GetInt("collisionCount");
    //                    //Console.WriteLine("contrib " + pillarContribution);
    //                }
    //                collided = true;
    //            }*/
    //            a += pillarContribution;
    //        }

    //        if (!collided)
    //        {
    //            island.SetInt("collisionCount", 0);
    //        }
    //        else
    //        {
    //            island.SetInt("collisionCount", island.GetInt("collisionCount") + 1);
    //        }

    //        // compute final acceleration
    //        island.SetVector3("acceleration", a);

    //        // compute final velocity
    //        v = (v + dt * island.GetVector3("acceleration")) * (1.0f - constants.GetFloat("damping"));
    //        float velocityLength = v.Length();
    //        Vector3 v_applied = v;
    //        if (velocityLength > constants.GetFloat("max_velocity"))
    //        {
    //            v_applied *= constants.GetFloat("max_velocity") / velocityLength;
    //        }
    //        island.SetVector3(CommonNames.Velocity, v);

    //        // compute final position
    //        island.SetVector3(CommonNames.Position, island.GetVector3(CommonNames.Position) + dt * v_applied);


    //        base.OnUpdate(island, simTime);
    //    }

    //    protected override void CollisionHandler(SimulationTime simTime, Entity island, Entity other, Contact co)
    //    {
    //        if (other.HasString(CommonNames.Kind) && other.GetString(CommonNames.Kind) == "pillar")
    //        {
    //            // todo: code here

    //        }
    //    }

    //    private Random rand;
    //}
}
