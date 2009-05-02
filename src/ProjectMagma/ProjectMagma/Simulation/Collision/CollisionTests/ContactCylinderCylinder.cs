using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    public class ContactCylinderCylinder
    {
        public static void Test(
            Entity entity1, object[] boundingVolumes1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, object[] boundingVolumes2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            Debug.Assert(scale1.X == scale1.Y && scale1.Y == scale1.Z);
            Debug.Assert(scale2.X == scale2.Y && scale2.Y == scale2.Z);

            for (int i = 0; i < boundingVolumes1.Length; ++i)
            {
                Cylinder3 cylinder1 = (Cylinder3)boundingVolumes1[i];

                for (int j = 0; j < boundingVolumes2.Length; ++j)
                {
                    Cylinder3 cylinder2 = (Cylinder3)boundingVolumes2[j];
                
                    Vector3 top1 = Vector3.Transform(cylinder1.Top, worldTransform1);
                    Vector3 bottom1 = Vector3.Transform(cylinder1.Bottom, worldTransform1);
                    Vector3 top2 = Vector3.Transform(cylinder2.Top, worldTransform2);
                    Vector3 bottom2 = Vector3.Transform(cylinder2.Bottom, worldTransform2);
                    float radius1 = scale1.X * cylinder1.Radius;
                    float radius2 = scale2.X * cylinder2.Radius;

                    float minTop = top1.Y < top2.Y ? top1.Y : top2.Y;
                    float maxBottom = bottom1.Y > bottom2.Y ? bottom1.Y : bottom2.Y;
                    float overlap = minTop - maxBottom;
                    if (overlap >= 0)
                    {
                        Vector3 projected1 = new Vector3(top1.X, 0, top1.Z);
                        Vector3 projected2 = new Vector3(top2.X, 0, top2.Z);
                        Vector3 normal = projected2 - projected1;
                        float radiusSum = radius1 + radius2;
                        if (normal.LengthSquared() < radiusSum * radiusSum)
                        {
                            // collision
                            normal.Normalize();
                            Vector3 position = projected1 + normal * radius1 + Vector3.UnitY * (minTop - overlap / 2.0f);
                            contact.AddContactPoint(ref position, ref normal);
                        }
                    }
                }
            }
        }
    }
}
