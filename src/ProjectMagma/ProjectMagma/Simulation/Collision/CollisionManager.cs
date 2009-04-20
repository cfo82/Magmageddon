using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Simulation.Collision.CollisionTests;

namespace ProjectMagma.Simulation.Collision
{
    public class CollisionManager
    {
        #region Construction and Destruction

        public CollisionManager()
        {
            this.testList = new TestList();

            threads = new CollisionThread[threadAffinities.Length];
            for (int i = 0; i < threadAffinities.Length; ++i)
            {
                threads[i] = new CollisionThread(threadAffinities[i], testList);
            }
        }

        public void Close()
        {
            foreach (CollisionThread thread in threads)
            {
                thread.Abort();
            }
        }

        #endregion

        #region Manage colliding entities

        public void AddCollisionEntity(Entity entity, CollisionProperty property, Cylinder3 cylinder, bool needAllContacts)
        {
            testList.Add(new CollisionEntity(entity, property, cylinder, needAllContacts));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, AlignedBox3Tree tree, bool needAllContacts)
        {
            testList.Add(new CollisionEntity(entity, property, tree, needAllContacts));
        }

        public void AddCollisionEntity(Entity entity, CollisionProperty property, Sphere3 sphere, bool needAllContacts)
        {
            testList.Add(new CollisionEntity(entity, property, sphere, needAllContacts));
        }

        public bool ContainsCollisionEntity(CollisionProperty property)
        {
            return testList.ContainsCollisionEntity(property);
        }

        public void RemoveCollisionEntity(CollisionProperty property)
        {
            testList.Remove(property);
        }

        #endregion

        #region Collision Detection

        //static int collisionCount = 0;

        public void Update(SimulationTime simTime)
        {
            //long t1 = System.DateTime.Now.Ticks;

            testList.BeginCollisionDetection();

            //long t2 = System.DateTime.Now.Ticks;
            
            foreach (CollisionThread t in threads)
            {
                t.Start();
            }

            //long t3 = System.DateTime.Now.Ticks;

            foreach (CollisionThread t in threads)
            {
                t.Join();
            }

            //long t4 = System.DateTime.Now.Ticks;

            testList.EndCollisionDetection();

            //long t5 = System.DateTime.Now.Ticks;

            foreach (CollisionThread t in threads)
            {
                //System.Console.WriteLine("collisions: " + t.ContactCount);

                for (int i = 0; i < t.ContactCount; ++i)
                {
                    Contact contact = t.GetContact(i);

                    //System.Console.WriteLine(contact.Count);
                    //if (
                    //    entry.EntityA.entity.HasString("kind") && entry.EntityB.entity.HasString("kind") &&(
                    //        entry.EntityA.entity.GetString("kind") == "pillar" ||
                    //        entry.EntityB.entity.GetString("kind") == "pillar" ||
                    //        (entry.EntityA.entity.GetString("kind") == "island" && entry.EntityB.entity.GetString("kind") == "island")
                    //    ))
                    //if ((contact.EntityA.HasString("kind") && contact.EntityA.GetString("kind") == "powerup") ||
                    //    (contact.EntityB.HasString("kind") && contact.EntityB.GetString("kind") == "powerup"))
                    //{
                    //    System.Console.WriteLine("Collision {0,4}: between {1} and {2}!", 0, contact.EntityA.Name, contact.EntityB.Name);
                    //}

                    CollisionProperty propertyA = contact.EntityA.HasProperty("collision") ? (CollisionProperty)contact.EntityA.GetProperty("collision") : null;
                    CollisionProperty propertyB = contact.EntityB.HasProperty("collision") ? (CollisionProperty)contact.EntityB.GetProperty("collision") : null;

                    if (propertyA != null && propertyB != null)
                    {
                        propertyA.FireContact(simTime, contact);
                        contact.Reverse();
                        propertyB.FireContact(simTime, contact);
                        //++collisionCount;
                    }
                    else
                    {
                        if (propertyA == null && propertyB == null)
                        {
                            throw new System.Exception(string.Format(
                                "someone has illegaly removed the collision property from entity {0} and {1}!",
                                contact.EntityA.Name, contact.EntityB.Name
                                ));
                        }
                        else
                        {
                            throw new System.Exception(string.Format(
                                "someone has illegaly removed the collision property from entity {0}!",
                                propertyA == null ? contact.EntityA.Name : contact.EntityB.Name
                                ));
                        }
                    }
                }
            }

            //long t6 = System.DateTime.Now.Ticks;

            //long dt1 = t2 - t1;
            //long dt2 = t3 - t2;
            //long dt3 = t4 - t3;
            //long dt4 = t5 - t4;
            //long dt5 = t6 - t5;

            //double ddt1 = (double)dt1 / 10000.0;
            //double ddt2 = (double)dt2 / 10000.0;
            //double ddt3 = (double)dt3 / 10000.0;
            //double ddt4 = (double)dt4 / 10000.0;
            //double ddt5 = (double)dt5 / 10000.0;

            //System.Console.WriteLine("collision detection timing: ");
            //System.Console.WriteLine("  dt1: {0:G}", ddt1);
            //System.Console.WriteLine("  dt2: {0:G}", ddt2);
            //System.Console.WriteLine("  dt3: {0:G}", ddt3);
            //System.Console.WriteLine("  dt4: {0:G}", ddt4);
            //System.Console.WriteLine("  dt5: {0:G} {1}", ddt5, ddt5>10.0?"***************************************************":"");
        }

        public static readonly ContactTest[,] ContactTests = new ContactTest[3, 3] {
            { ContactCylinderCylinder.Test, ContactCylinderMesh.Test, ContactCylinderSphere.Test },
            { ContactMeshCylinder.Test, ContactMeshMeshBox.Test, ContactMeshSphere.Test },
            { ContactSphereCylinder.Test, ContactSphereMesh.Test, ContactSphereSphere.Test } 
        };

        #endregion

        #region Picking

        /// <summary>
        /// this method is a bit hacky since I did not know where to place it. I may move it to a better
        /// place somewhen in the future
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="entity"></param>
        public bool GetIntersectionPoint(
            ref Ray3 ray,
            Entity entity,
            out Vector3 outIsectPt
        )
        {
            // find the matching collision entity (in order to get the alignedbox3tree
            CollisionEntity collisionEntity = testList.GetCollisionEntity(entity);
            if (collisionEntity == null)
            {
                outIsectPt = Vector3.Zero;
                return false;
            }

            Debug.Assert(collisionEntity.VolumeType == VolumeType.AlignedBox3Tree);
            if (collisionEntity.VolumeType != VolumeType.AlignedBox3Tree)
            {
                outIsectPt = Vector3.Zero;
                return false;
            }

            // get world transform of from the given entity
            Matrix worldTransform;
            AlignedBox3Tree tree = (AlignedBox3Tree)collisionEntity.Volume;
            OrientationHelper.CalculateWorldTransform(entity, out worldTransform);

            // transform the ray into the coordinate system of the entity
            Matrix worldToEntityTransform = Matrix.Invert(worldTransform);
            Matrix inverseTransposeWorldToEntityTransform = Matrix.Transpose(Matrix.Invert(worldToEntityTransform));
            Vector3 entitySpaceRayOrigin = Vector3.Transform(ray.Origin, worldToEntityTransform);
            Vector3 entitySpaceRayDirection = Vector3.Transform(ray.Direction, inverseTransposeWorldToEntityTransform);
            entitySpaceRayDirection.Normalize();
            Ray3 entitySpaceRay = new Ray3(entitySpaceRayOrigin, entitySpaceRayDirection);

            float t; // parameter on ray, outIsectPt = ray.Origin + t * ray.Direction;
            bool intersection = GetIntersectionPoint(ref entitySpaceRay, tree.Root, tree.Positions, out t);
            if (intersection)
            {
                outIsectPt = Vector3.Transform(entitySpaceRay.Origin + t * entitySpaceRay.Direction, worldTransform);
            }
            else
            {
                outIsectPt = Vector3.Zero;
            }
            return intersection;
        }

        private bool GetIntersectionPoint(
            ref Ray3 ray,
            AlignedBox3TreeNode node,
            Vector3[] positions,
            out float t
        )
        {
            AlignedBox3 boundingBox = node.BoundingBox;
            if (!Intersection.IntersectRay3AlignedBox3(ref ray, ref boundingBox))
            {
                t = 0.0f;
                return false;
            }

            if (node.HasChildren)
            {
                float t1, t2;
                bool isect1 = GetIntersectionPoint(ref ray, node.Left, positions, out t1);
                bool isect2 = GetIntersectionPoint(ref ray, node.Right, positions, out t2);

                if (isect1 && isect2)
                {
                    t = t1 < t2 ? t1 : t2;
                    return true;
                }
                else if (isect1)
                {
                    t = t1;
                    return true;
                }
                else if (isect2)
                {
                    t = t2;
                    return true;
                }
                else
                {
                    t = 0.0f;
                    return false;
                }
            }
            else
            {
                bool intersection = false;
                float smallestT = float.MaxValue;
                for (int i = 0; i < node.NumTriangles; ++i)
                {
                    float currentT = 0.0f;
                    Vector3 isectPt = Vector3.Zero;
                    Triangle3 tri = node.GetTriangle(positions, i);
                    if (Intersection.IntersectRay3Triangle3(ref ray, ref tri, out currentT, out isectPt))
                    {
                        if (currentT < smallestT)
                        {
                            smallestT = currentT;
                        }

                        if (!intersection)
                            { intersection = true; }
                    }
                }

                if (intersection)
                {
                    t = smallestT;
                    return true;
                }
                else
                {
                    t = 0.0f;
                    return false;
                }
            }
        }

        #endregion

        #region Member Variables

        private readonly int[] threadAffinities = new int[] { 1, 3, 5 };
        private TestList testList;
        private CollisionThread[] threads;

        #endregion
    }
}
