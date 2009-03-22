using System.Diagnostics;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;
using ProjectMagma.Shared.BoundingVolume;

namespace ProjectMagma.Collision.CollisionTests
{
    public class ContactSphereCylinder
    {
        public static Contact Test(
            Entity entity1, object boundingVolume1,
            Entity entity2, object boundingVolume2
            )
        {
            BoundingSphere sphere1 = (BoundingSphere)boundingVolume1;
            BoundingCylinder cylinder2 = (BoundingCylinder)boundingVolume2;

            // sphere is on same level as the cylinder
            if (sphere1.Center.Y < cylinder2.Top.Y &&
                sphere1.Center.Y > cylinder2.Bottom.Y)
            {
                // distance between the two
                Vector3 diff = cylinder2.Top - sphere1.Center;
                diff.Y = 0; // we are only interested in horizontal distance
                float collisionLengthSquared = (cylinder2.Radius + sphere1.Radius) * (cylinder2.Radius + sphere1.Radius);
                if (collisionLengthSquared < diff.LengthSquared())
                {
                    Contact c = new Contact();
                    c.entityA = entity1;
                    c.entityB = entity2;
                    c.normal = diff;
                    c.normal.Normalize();
                    c.position = sphere1.Center + c.normal * sphere1.Radius;
                    return c;
                }
            }
            // above cylinder...
            else if (sphere1.Center.Y > cylinder2.Top.Y)
            {
                if (sphere1.Center.Y - sphere1.Radius < cylinder2.Top.Y)
                {
                    // project to top cylinder 'plane'
                    Vector3 projected = sphere1.Center;
                    projected.Y = cylinder2.Top.Y;

                    Vector3 toProjected = projected - cylinder2.Top;
                    if (toProjected.LengthSquared() < cylinder2.Radius * cylinder2.Radius)
                    {
                        Contact c = new Contact();
                        c.entityA = entity1;
                        c.entityB = entity2;
                        c.normal = -Vector3.UnitY;
                        c.position = projected;
                        return c;
                    }
                    else
                    {
                        toProjected.Normalize();
                        Vector3 nearestPoint = cylinder2.Top + toProjected * cylinder2.Radius;
                        Vector3 normal = nearestPoint - sphere1.Center;
                        if (normal.LengthSquared() < sphere1.Radius * sphere1.Radius)
                        {
                            Contact c = new Contact();
                            c.entityA = entity1;
                            c.entityB = entity2;
                            c.normal = normal;
                            c.normal.Normalize();
                            c.position = nearestPoint;
                        }
                    }
                }
            }
            // below cylinder
            else if (sphere1.Center.Y < cylinder2.Bottom.Y)
            {
                if (sphere1.Center.Y + sphere1.Radius < cylinder2.Bottom.Y)
                {
                    // project to bottom cylinder 'plane'
                    Vector3 projected = sphere1.Center;
                    projected.Y = cylinder2.Bottom.Y;

                    Vector3 toProjected = projected - cylinder2.Bottom;
                    if (toProjected.LengthSquared() < cylinder2.Radius * cylinder2.Radius)
                    {
                        Contact c = new Contact();
                        c.entityA = entity1;
                        c.entityB = entity2;
                        c.normal = Vector3.UnitY;
                        c.position = projected;
                        return c;
                    }
                    else
                    {
                        toProjected.Normalize();
                        Vector3 nearestPoint = cylinder2.Bottom + toProjected * cylinder2.Radius;
                        Vector3 normal = nearestPoint - sphere1.Center;
                        if (normal.LengthSquared() < sphere1.Radius * sphere1.Radius)
                        {
                            Contact c = new Contact();
                            c.entityA = entity1;
                            c.entityB = entity2;
                            c.normal = normal;
                            c.normal.Normalize();
                            c.position = nearestPoint;
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
