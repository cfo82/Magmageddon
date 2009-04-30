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
            this.startEvent = new AutoResetEvent(false);
            this.finishedEvent = new AutoResetEvent(false);
            this.aborted = false;
            this.processor = processor;
            this.testList = testList;
            this.contacts = new List<Contact>();

            this.thread = new Thread(Run);
            this.thread.Name = "CollisionThread" + processor;
            this.thread.Start();
        }

        public void Start()
        {
            contacts.Clear();
            startEvent.Set();
        }

        private void Run()
        {
#if XBOX
            this.thread.SetProcessorAffinity(processor);
#endif
            try
            {
                while (true)
                {
                    startEvent.WaitOne();

                    TestList.TestEntry entry = testList.GetNextCollisionEntry();
                    while (entry != null)
                    {
                        // do collision detection with this entry!
                        ContactTest test = CollisionManager.ContactTests[
                            BoundingVolumeTypeUtil.ToNumber(entry.EntityA.VolumeType),
                            BoundingVolumeTypeUtil.ToNumber(entry.EntityB.VolumeType)
                            ];


                        Vector3 scale1, position1, scale2, position2;
                        Quaternion rotation1, rotation2;
                        Matrix worldTransform1, worldTransform2;
                        OrientationHelper.GetTranslation(entry.EntityA.Entity, out position1);
                        OrientationHelper.GetRotation(entry.EntityA.Entity, out rotation1);
                        OrientationHelper.GetScale(entry.EntityA.Entity, out scale1);
                        OrientationHelper.CalculateWorldTransform(ref position1, ref rotation1, ref scale1, out worldTransform1);

                        OrientationHelper.GetTranslation(entry.EntityB.Entity, out position2);
                        OrientationHelper.GetRotation(entry.EntityB.Entity, out rotation2);
                        OrientationHelper.GetScale(entry.EntityB.Entity, out scale2);
                        OrientationHelper.CalculateWorldTransform(ref position2, ref rotation2, ref scale2, out worldTransform2);

                        Contact contact = new Contact(entry.EntityA.Entity, entry.EntityB.Entity);

                        test(
                            entry.EntityA.Entity, entry.EntityA.Volumes, ref worldTransform1, ref position1, ref rotation1, ref scale1,
                            entry.EntityB.Entity, entry.EntityB.Volumes, ref worldTransform2, ref position2, ref rotation2, ref scale2,
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

                    finishedEvent.Set();
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
            finishedEvent.WaitOne();
        }

        public void Abort()
        {
            this.aborted = true;
            this.thread.Abort();
        }

        public int ContactCount
        {
            get { return contacts.Count; }
        }

        public Contact GetContact(int index)
        {
            return contacts[index];
        }


        private AutoResetEvent startEvent;
        private AutoResetEvent finishedEvent;
        private bool aborted;
        private int processor;
        private TestList testList;
        private Thread thread;
        private List<Contact> contacts;
    }
}
