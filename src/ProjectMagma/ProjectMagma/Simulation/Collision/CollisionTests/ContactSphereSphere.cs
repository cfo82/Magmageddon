using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    public class ContactSphereSphere
    {
        public static void Test(
            Entity entity1, object boundingVolume1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, object boundingVolume2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
            )
        {
            Sphere3 sphere1 = (Sphere3)boundingVolume1;
            Sphere3 sphere2 = (Sphere3)boundingVolume2;

            Debug.Assert(scale1.X == scale1.Y && scale1.Y == scale1.Z);
            Debug.Assert(scale2.X == scale2.Y && scale2.Y == scale2.Z);

            Vector3 center1 = Vector3.Transform(sphere1.Center, worldTransform1);
            float radius1 = scale1.X * sphere1.Radius;
            Vector3 center2 = Vector3.Transform(sphere2.Center, worldTransform2);
            float radius2 = scale2.X * sphere2.Radius;

            Vector3 diff = center2 - center1;
            if (diff.LengthSquared() < (radius1 + radius2) * (radius1 + radius2))
            {
                diff.Normalize();
                Vector3 point = center1 + diff * radius1;
                contact.AddContactPoint(ref point, ref diff);
            }
        }
    }
}
