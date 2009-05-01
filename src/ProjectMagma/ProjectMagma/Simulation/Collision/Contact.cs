using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Collision
{
    public class Contact : IEnumerable<ContactPoint>
    {
        public Contact(
            Entity entityA,
            Entity entityB
        )
        {
            this.entityA = entityA;
            this.entityB = entityB;
            //contactPoints = new List<ContactPoint>();
            cp = new ContactPoint();
        }

        public void Reverse()
        {
            Entity temp = entityB;
            entityB = entityA;
            entityA = temp;

            /*for (int i = 0; i < contactPoints.Count; ++i)
            {
                contactPoints[i] = new ContactPoint(contactPoints[i].Point, -contactPoints[i].Normal);
            }*/
            cp = new ContactPoint(cp.Point, -cp.Normal);
        }

        public bool ContainsPoint(
            ref Vector3 p
        )
        {
            /*for (int i = 0; i < contactPoints.Count; ++i)
            {
                if (contactPoints[i].Point == p)
                    { return true; }
            }*/
            return false;
        }

        public void AddContactPoint(
            ref Vector3 point,
            ref Vector3 normal
        )
        {
            //contactPoints.Add(new ContactPoint(point, normal));
            cp = new ContactPoint(point, normal);
            hasContact = true;
        }

        public Entity EntityA
        {
            get { return entityA; }
        }

        public Entity EntityB
        {
            get { return entityB; }
        }

        public int Count
        {
            get
            {
                return hasContact?1:0;// return contactPoints.Count;
            }
        }

        public ContactPoint this[int index]
        {
            get
            {
                Debug.Assert(index == 0);
                return cp;
                //return contactPoints[index];
            }
        }

        #region Implement IEnumerable interface

        private class ContactEnumerator : IEnumerator<ContactPoint>
        {
            public ContactEnumerator(Contact contact)
            {
                this.contact = contact;
                this.index = -1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ++index;
                return index < contact.Count;
            }

            public void Reset()
            {
                index = -1;
            }

            public ContactPoint Current
            {
                get
                {
                    return contact[index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return contact[index];
                }
            }

            private Contact contact;
            private int index = 0;
        };

        public IEnumerator<ContactPoint> GetEnumerator()
        {
            return new ContactEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ContactEnumerator(this);
        }

        #endregion

        private Entity entityA;
        private Entity entityB;
        //private List<ContactPoint> contactPoints;
        private ContactPoint cp;
        private bool hasContact;
    }
}
