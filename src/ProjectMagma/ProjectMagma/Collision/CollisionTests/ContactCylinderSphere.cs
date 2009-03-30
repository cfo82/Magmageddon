using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma.Collision.CollisionTests
{
    class ContactCylinderSphere
    {
        public static void Test(
            Entity entity1, object boundingVolume1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, object boundingVolume2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts
            )
        {
            ContactSphereCylinder.Test(
                entity2, boundingVolume2, worldTransform2, translation2, rotation2, scale2,
                entity1, boundingVolume1, worldTransform1, translation1, rotation1, scale1,
                contacts
                );
            foreach (Contact c in contacts)
            {
                c.Reverse();
            }
        }
    }
}
