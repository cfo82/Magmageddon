using System.Diagnostics;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Collision.CollisionTests
{
    public class ContactSphereCylinder
    {
        public static Contact Test(
            Entity entity1, object boundingVolume1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, object boundingVolume2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2
            )
        {
            Sphere3 sphere1 = (Sphere3)boundingVolume1;
            Cylinder3 cylinder2 = (Cylinder3)boundingVolume2;

            Debug.Assert(scale1.X == scale1.Y && scale1.Y == scale1.Z);
            Debug.Assert(scale2.X == scale2.Y && scale2.Y == scale2.Z);

            Vector3 center1 = Vector3.Transform(sphere1.Center, worldTransform1);
            float radius1 = scale1.X * sphere1.Radius;
            Vector3 top2 = Vector3.Transform(cylinder2.Top, worldTransform2);
            Vector3 bottom2 = Vector3.Transform(cylinder2.Bottom, worldTransform2);
            float radius2 = scale2.X * cylinder2.Radius;

            // sphere is on same level as the cylinder
            if (center1.Y <= top2.Y &&
                center1.Y >= bottom2.Y)
            {
                // distance between the two
                Vector3 diff = top2 - center1;
                diff.Y = 0; // we are only interested in horizontal distance
                float collisionLengthSquared = (radius2 + radius1) * (radius2 + radius1);
                if (diff.LengthSquared() < collisionLengthSquared)
                {
                    diff.Normalize();
                    Contact c = new Contact(entity1, entity2, center1 + diff * radius1, diff);
                    return c;
                }
            }
            // above cylinder...
            else if (center1.Y > top2.Y)
            {
                if (center1.Y - radius1 < top2.Y)
                {
                    // project to top cylinder 'plane'
                    Vector3 projected = center1;
                    projected.Y = top2.Y;

                    Vector3 toProjected = projected - top2;
                    if (toProjected.LengthSquared() < radius2 * radius2)
                    {
                        Contact c = new Contact(entity1, entity2, projected, -Vector3.UnitY);
                        return c;
                    }
                    else
                    {
                        toProjected.Normalize();
                        Vector3 nearestPoint = top2 + toProjected * radius2;
                        Vector3 diff = nearestPoint - center1;
                        if (diff.LengthSquared() < radius1 * radius1)
                        {
                            Contact c = new Contact(entity1, entity2, nearestPoint, -Vector3.UnitY);
                            return c;
                        }
                    }
                }
            }
            // below cylinder
            else if (center1.Y < bottom2.Y)
            {
                if (center1.Y + radius1 < bottom2.Y)
                {
                    // project to bottom cylinder 'plane'
                    Vector3 projected = center1;
                    projected.Y = bottom2.Y;

                    Vector3 toProjected = projected - bottom2;
                    if (toProjected.LengthSquared() < radius2 * radius2)
                    {
                        Contact c = new Contact(entity1, entity2, projected, Vector3.UnitY);
                        return c;
                    }
                    else
                    {
                        toProjected.Normalize();
                        Vector3 nearestPoint = bottom2 + toProjected * radius2;
                        Vector3 diff = nearestPoint - center1;
                        if (diff.LengthSquared() < radius1 * radius1)
                        {
                            Contact c = new Contact(entity1, entity2, projected, Vector3.UnitY);
                            return c;
                        }
                    }
                }
            }
            else
            {
                // we covered all cases...
                Debug.Assert(false);
            }

            return null;
        }

    }
}
