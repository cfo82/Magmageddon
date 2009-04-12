using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Simulation.Collision.CollisionTests;

namespace ProjectMagma.Simulation.Collision
{
    class CollisionThread
    {
        public CollisionThread(int processor, TestList testList)
        {
            this.startEvent = new object();
            this.finishedEvent = new object();
            this.started = false;
            this.finished = false;
            this.aborted = false;
            this.processor = processor;
            this.testList = testList;
            this.contacts = new List<Contact>();

            this.thread = new Thread(Run);
            this.thread.Name = "CollisionThread" + processor;
#if XBOX
            this.thread.SetProcessorAffinity(new int[]{processor});
#endif
            this.thread.Start();
        }

        public void Start()
        {
            contacts.Clear();
            lock (startEvent)
            {
                started = true;
                Monitor.Pulse(startEvent);
            }
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    lock (startEvent)
                    {
                        if (!started)
                        {
                            Monitor.Wait(startEvent);
                        }
                        started = false;
                    }

                    TestList.TestEntry entry = testList.GetNextCollisionEntry();
                    while (entry != null)
                    {
                        // do collision detection with this entry!
                        ContactTest test = contactTests[BoundingVolumeTypeUtil.ToNumber(entry.EntityA.VolumeType), BoundingVolumeTypeUtil.ToNumber(entry.EntityB.VolumeType)];

                        Matrix worldTransform1;
                        Vector3 position1 = GetPosition(entry.EntityA.Entity);
                        Quaternion rotation1 = GetRotation(entry.EntityA.Entity);
                        Vector3 scale1 = GetScale(entry.EntityA.Entity);
                        CalculateWorldTransform(ref position1, ref rotation1, ref scale1, out worldTransform1);

                        Matrix worldTransform2;
                        Vector3 position2 = GetPosition(entry.EntityB.Entity);
                        Quaternion rotation2 = GetRotation(entry.EntityB.Entity);
                        Vector3 scale2 = GetScale(entry.EntityB.Entity);
                        CalculateWorldTransform(ref position2, ref rotation2, ref scale2, out worldTransform2);

                        Contact contact = new Contact(entry.EntityA.Entity, entry.EntityB.Entity);

                        test(
                            entry.EntityA.Entity, entry.EntityA.Volume, ref worldTransform1, ref position1, ref rotation1, ref scale1,
                            entry.EntityB.Entity, entry.EntityB.Volume, ref worldTransform2, ref position2, ref rotation2, ref scale2,
                            entry.EntityA.NeedAllContacts || entry.EntityB.NeedAllContacts, ref contact
                            );
                        if (contact.Count > 0)
                        {
                            Debug.Assert(contact.EntityA == entry.EntityA.Entity && contact.EntityB == entry.EntityB.Entity);
                            contacts.Add(contact);
                        }

                        // get next entry
                        entry = testList.GetNextCollisionEntry();
                    }

                    lock (finishedEvent)
                    {
                        finished = true;
                        Monitor.Pulse(finishedEvent);
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                if (!this.aborted)
                {
                    System.Console.WriteLine("unexpected ThreadAbortException {0}", ex);
                    throw ex;
                }
            }
        }

        public void Join()
        {
            lock (finishedEvent)
            {
                if (!finished)
                {
                    Monitor.Wait(finishedEvent);
                }
                finished = false;
            }
        }

        public void Abort()
        {
            this.aborted = true;
            this.thread.Abort();
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

        public int ContactCount
        {
            get { return contacts.Count; }
        }

        public Contact GetContact(int index)
        {
            return contacts[index];
        }

        private object startEvent;
        private object finishedEvent;
        private bool started;
        private bool finished;
        private bool aborted;
        private int processor;
        private TestList testList;
        private Thread thread;
        private List<Contact> contacts = new List<Contact>();

        private static readonly ContactTest[,] contactTests = new ContactTest[3, 3] {
            { ContactCylinderCylinder.Test, ContactCylinderMesh.Test, ContactCylinderSphere.Test },
            { ContactMeshCylinder.Test, ContactMeshMeshBox.Test, ContactMeshSphere.Test },
            { ContactSphereCylinder.Test, ContactSphereMesh.Test, ContactSphereSphere.Test } 
        };
    }
}
