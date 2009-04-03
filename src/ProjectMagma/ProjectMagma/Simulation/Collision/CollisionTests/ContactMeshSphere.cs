using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Distance;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    class ContactMeshSphere
    {
        private static void Test(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, Sphere3 sphere2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts
        )
        {
            /*
	            math::Matrix4x4<TypeT> translation_matrix;
	            translation_matrix.ToTranslate(translation);
	            math::Matrix4x4<TypeT> world_matrix = translation_matrix * rotation_matrix;

	            // TODO: fix this early exit code!!
	            math::Box3<TypeT> box;
	            box.SetCenter(world_matrix * node->GetBoundingBox().GetCenter());
	            box.SetExtents(node->GetBoundingBox().GetExtents());
	            box.SetAxis(rotation_matrix.GetRow(0), 0);
	            box.SetAxis(rotation_matrix.GetRow(1), 1);
	            box.SetAxis(rotation_matrix.GetRow(2), 2);

	            math::Vector3<TypeT> vertices[8];
	            box.ComputeVertices(vertices);

	            if (!intersect_orientedbox_sphere(box, sphere))
	            {
		            return;
	            }
            */

            /*
                math::Vector3<TypeT> min = node->GetBoundingBox().GetMin();
                math::Vector3<TypeT> max = node->GetBoundingBox().GetMax();

                math::Vector3r reference_vertices[8] = {
                    math::Vector3r(min.x, min.y, min.z),
                    math::Vector3r(max.x, min.y, min.z),
                    math::Vector3r(min.x, max.y, min.z),
                    math::Vector3r(max.x, max.y, min.z),
                    math::Vector3r(min.x, min.y, max.z),
                    math::Vector3r(max.x, min.y, max.z),
                    math::Vector3r(min.x, max.y, max.z),
                    math::Vector3r(max.x, max.y, max.z)
                };
                for (int i = 0; i < 8; ++i)
                {
                    reference_vertices[i] = world_matrix * reference_vertices[i];
                }

                for (int i = 0; i < 8; ++i)
                {
                    bool found = false;
                    for (int j = 0; j < 8; ++j)
                    {
                        if (vertices[i] == reference_vertices[j])
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        int dummy;
                        dummy = 7;
                        printf("%d\n", dummy);
                    }
                }
            */

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
                        bool newContact = true;
                        foreach (Contact c in contacts)
                        { newContact = newContact && c.Position != closestPoint; }

                        if (newContact)
                        {
                            contacts.Add(new Contact(entity1, entity2, closestPoint, tri.Normal));
                        }
                    }
                }
            }
            else
            {
                Test(
                    entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, sphere2, worldTransform2, translation2, rotation2, scale2,
                    contacts
                    );
                Test(
                    entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, sphere2, worldTransform2, translation2, rotation2, scale2,
                    contacts
                    );
            }
        }

        public static void Test(
            Entity entity1, object boundingVolume1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, object boundingVolume2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts
            )
        {
            AlignedBox3Tree tree1 = (AlignedBox3Tree)boundingVolume1;
            Sphere3 sphere2 = (Sphere3)boundingVolume2;

            Debug.Assert(scale2.X == scale2.Y && scale2.Y == scale2.Z);
            Sphere3 transformedSphere2 = new Sphere3(Vector3.Transform(sphere2.Center, worldTransform2), sphere2.Radius * scale2.X);

            Test(entity1, tree1.Root, tree1.Positions, worldTransform1, translation1, rotation1, scale1,
                entity2, transformedSphere2, worldTransform2, translation2, rotation2, scale2,
                contacts);
        }
    }
}
