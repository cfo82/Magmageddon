using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Volume;
using ProjectMagma.Simulation.Collision.CollisionTests;

namespace ProjectMagma.Simulation.Collision
{
    public class CollisionManager
    {
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
                    //{
                    //    System.Console.WriteLine("Collision {0,4}: between {1} and {2}!", collisionCount, entry.EntityA.Entity.Name, entry.EntityB.Entity.Name);
                    //}

                    CollisionProperty propertyA = (CollisionProperty)contact.EntityA.GetProperty("collision");
                    CollisionProperty propertyB = (CollisionProperty)contact.EntityB.GetProperty("collision");

                    propertyA.FireContact(simTime, contact);
                    contact.Reverse();
                    propertyB.FireContact(simTime, contact);
                    //++collisionCount;
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

        private void CalculateWorldTransform(
            ref Vector3 translation,
            ref Quaternion rotation,
            ref Vector3 scale,
            out Matrix world
        )
        {
            Matrix scaleMatrix, rotationMatrix, translationMatrix, tempMatrix;
            Matrix.CreateScale(ref scale, out scaleMatrix);
            Matrix.CreateFromQuaternion(ref rotation, out rotationMatrix);
            Matrix.CreateTranslation(ref translation, out translationMatrix);
            Matrix.Multiply(ref scaleMatrix, ref rotationMatrix, out tempMatrix);
            Matrix.Multiply(ref tempMatrix, ref translationMatrix, out world);
        }

        private Vector3 GetPosition(Entity entity)
        {
            return entity.GetVector3("position");
        }

        private Vector3 GetScale(Entity entity)
        {
            if (entity.HasVector3("scale"))
            {
                return entity.GetVector3("scale");
            }
            else
            {
                return Vector3.One;
            }
        }

        private Quaternion GetRotation(Entity entity)
        {
            if (entity.HasQuaternion("rotation"))
            {
                return entity.GetQuaternion("rotation");
            }
            else
            {
                return Quaternion.Identity;
            }
        }

        private readonly ContactTest[,] contactTests = new ContactTest[3, 3] {
            { ContactCylinderCylinder.Test, ContactCylinderMesh.Test, ContactCylinderSphere.Test },
            { ContactMeshCylinder.Test, ContactMeshMeshBox.Test, ContactMeshSphere.Test },
            { ContactSphereCylinder.Test, ContactSphereMesh.Test, ContactSphereSphere.Test } 
        };

        private readonly int[] threadAffinities = new int[] { 1, 3, 5 };

        private TestList testList;
        private CollisionThread[] threads;
    }
}
