using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math;
using ProjectMagma.Shared.Math.Distance;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    class ContactMeshSphere
    {
        private static void Test(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, Sphere3 sphere2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            /*Matrix identityMatrix = Matrix.Identity;
            Box3 box1 = node1.BoundingBox.CreateBox3(ref worldTransform1);
            Box3 box2 = sphere2.CreateBox3(ref identityMatrix);
            if (!Intersection.IntersectBox3Box3(box1, box2))
            {
                return;
            }*/

            // see if this node has some primitives => has no children
            if (!node1.HasChildren)
            {
                // test here for all triangles...
                int tricount = node1.NumTriangles;
                for (int i = 0; i < tricount; ++i)
                {
                    //size_t contacts_size = contacts.size();
                    Triangle3 tri = node1.GetTriangle(positions1, i);
                    tri.Vertex0 = Vector3.Transform(tri.Vertex0, worldTransform1);
                    tri.Vertex1 = Vector3.Transform(tri.Vertex1, worldTransform1);
                    tri.Vertex2 = Vector3.Transform(tri.Vertex2, worldTransform1);

                    Vector3 sphereCenter = sphere2.Center;
                    Vector3 closestPoint = Vector3.Zero;
                    float squaredDistance = SquaredDistance.Vector3Triangle3(ref sphereCenter, ref tri, out closestPoint);
                    if (squaredDistance < sphere2.Radius * sphere2.Radius)
                    {
                        if (!contact.ContainsPoint(ref closestPoint))
                        {
                            Vector3 normal = tri.Normal;
                            contact.AddContactPoint(ref closestPoint, ref normal);
                        }

                        if (!needAllContacts)
                        {
                            return;
                        }
                    }
                }
            }
            else
            {
                Test(
                    entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, sphere2, worldTransform2, translation2, rotation2, scale2,
                    needAllContacts, ref contact
                    );

                if (!needAllContacts && contact.Count > 0)
                {
                    return;
                }

                Test(
                    entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, sphere2, worldTransform2, translation2, rotation2, scale2,
                    needAllContacts, ref contact
                    );
            }
        }

        public static void Test(
            Entity entity1, object boundingVolume1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, object boundingVolume2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            AlignedBox3Tree tree1 = (AlignedBox3Tree)boundingVolume1;
            Sphere3 sphere2 = (Sphere3)boundingVolume2;

            Debug.Assert(scale2.X == scale2.Y && scale2.Y == scale2.Z);
            Sphere3 transformedSphere2 = new Sphere3(Vector3.Transform(sphere2.Center, worldTransform2), sphere2.Radius * scale2.X);

            Test(entity1, tree1.Root, tree1.Positions, worldTransform1, translation1, rotation1, scale1,
                entity2, transformedSphere2, worldTransform2, translation2, rotation2, scale2,
                needAllContacts, ref contact);
        }
    }
}
