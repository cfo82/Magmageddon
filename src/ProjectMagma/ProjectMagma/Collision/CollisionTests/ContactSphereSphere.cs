using System.Diagnostics;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma.Collision.CollisionTests
{
    public class ContactSphereSphere
    {
        public static Contact Test(
            Entity entity1, object boundingVolume1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, object boundingVolume2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2
            )
        {
            BoundingSphere sphere1 = (BoundingSphere)boundingVolume1;
            BoundingSphere sphere2 = (BoundingSphere)boundingVolume2;

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
                Contact c = new Contact(entity1, entity2, center1 + diff * radius1, diff);
                return c;
            }

            return null;
        }
    }
}
