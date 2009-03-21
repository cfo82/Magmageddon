using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Shared.BoundingVolume;

namespace ProjectMagma.CollisionManager.CollisionTests
{
    public class ContactSphereSphere
    {
        public static Contact Test(
            Entity entity1, object boundingVolume1,
            Entity entity2, object boundingVolume2
            )
        {
            BoundingSphere sphere1 = (BoundingSphere)boundingVolume1;
            BoundingSphere sphere2 = (BoundingSphere)boundingVolume2;

            Vector3 diff = sphere2.Center - sphere1.Center;
            if (diff.LengthSquared() < (sphere1.Radius + sphere2.Radius) * (sphere1.Radius + sphere2.Radius))
            {
                Contact c = new Contact();
                c.entityA = entity1;
                c.entityB = entity2;
                c.normal = diff;
                c.normal.Normalize();
                c.position = sphere1.Center + c.normal * sphere1.Radius;
                return c;
            }

            return null;
        }
    }
}
