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
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, ref Sphere3 sphere2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            /*Sphere3 sphere1;
            node1.BoundingBox.CreateSphere3(ref worldTransform1, out sphere1);
            Vector3 diff = sphere1.Center - sphere2.Center;
            if (diff.LengthSquared() > (sphere1.Radius + sphere2.Radius) * (sphere1.Radius + sphere2.Radius))
            {
                return;
            }
            */
            Box3 box1, box2;
            Matrix identityMatrix = Matrix.Identity;
            node1.BoundingBox.CreateBox3(ref worldTransform1, out box1);
            sphere2.CreateBox3(ref identityMatrix, out box2);
            if (!Intersection.IntersectBox3Box3(ref box1, ref box2))
            {
                return;
            }

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
                    entity1, node1.Left, positions1, ref worldTransform1, ref translation1, ref rotation1, ref scale1,
                    entity2, ref sphere2, ref worldTransform2, ref translation2, ref rotation2, ref scale2,
                    needAllContacts, ref contact
                    );

                if (!needAllContacts && contact.Count > 0)
                {
                    return;
                }

                Test(
                    entity1, node1.Right, positions1, ref worldTransform1, ref translation1, ref rotation1, ref scale1,
                    entity2, ref sphere2, ref worldTransform2, ref translation2, ref rotation2, ref scale2,
                    needAllContacts, ref contact
                    );
            }
        }

        public static void Test(
            Entity entity1, object[] boundingVolumes1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, object[] boundingVolumes2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            for (int i = 0; i < boundingVolumes1.Length; ++i)
            {
                AlignedBox3Tree tree1 = (AlignedBox3Tree)boundingVolumes1[i];

                for (int j = 0; j < boundingVolumes2.Length; ++j)
                {
                    Sphere3 sphere2 = (Sphere3)boundingVolumes2[j];

                    Debug.Assert(scale2.X == scale2.Y && scale2.Y == scale2.Z);
                    Sphere3 transformedSphere2 = new Sphere3(Vector3.Transform(sphere2.Center, worldTransform2), sphere2.Radius * scale2.X);

                    Test(entity1, tree1.Root, tree1.Positions, ref worldTransform1, ref translation1, ref rotation1, ref scale1,
                        entity2, ref transformedSphere2, ref worldTransform2, ref translation2, ref rotation2, ref scale2,
                        needAllContacts, ref contact);
                }
            }
        }
    }
}
