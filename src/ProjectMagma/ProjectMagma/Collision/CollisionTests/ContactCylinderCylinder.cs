using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Shared.BoundingVolume;

namespace ProjectMagma.Collision.CollisionTests
{
    public class ContactCylinderCylinder
    {
        public static Contact Test(
            Entity entity1, object boundingVolume1,
            Entity entity2, object boundingVolume2
            )
        {
            BoundingCylinder cylinder1 = (BoundingCylinder)boundingVolume1;
            BoundingCylinder cylinder2 = (BoundingCylinder)boundingVolume2;

            float minTop = cylinder1.Top.Y < cylinder2.Top.Y ? cylinder1.Top.Y : cylinder2.Top.Y;
            float maxBottom = cylinder1.Bottom.Y > cylinder2.Bottom.Y ? cylinder1.Bottom.Y : cylinder2.Bottom.Y;
            float overlap = minTop - maxBottom;
            if (overlap >= 0)
            {
                Vector3 projected1 = new Vector3(cylinder1.Top.X, 0, cylinder1.Top.Z);
                Vector3 projected2 = new Vector3(cylinder2.Top.X, 0, cylinder2.Top.Z);
                Vector3 normal = projected2 - projected1;
                float radiusSum = cylinder1.Radius + cylinder2.Radius;
                if (normal.LengthSquared() < radiusSum * radiusSum)
                {
                    // collision
                    Contact c = new Contact();
                    c.entityA = entity1;
                    c.entityB = entity2;
                    c.normal = normal;
                    c.normal.Normalize();
                    c.position = projected1 + c.normal * cylinder1.Radius + Vector3.UnitY * (minTop - overlap / 2.0f);
                    return c;
                }
            }

            return null;
        }
    }
}
