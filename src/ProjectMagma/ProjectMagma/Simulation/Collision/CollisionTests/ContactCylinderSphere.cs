using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    class ContactCylinderSphere
    {
        public static void Test(
            Entity entity1, object boundingVolume1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, object boundingVolume2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            contact.Reverse();
            ContactSphereCylinder.Test(
                entity2, boundingVolume2, ref worldTransform2, ref translation2, ref rotation2, ref scale2,
                entity1, boundingVolume1, ref worldTransform1, ref translation1, ref rotation1, ref scale1,
                needAllContacts, ref contact
                );
            contact.Reverse();
        }
    }
}
