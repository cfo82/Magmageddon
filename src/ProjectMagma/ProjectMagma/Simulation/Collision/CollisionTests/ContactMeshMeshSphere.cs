using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using ProjectMagma.Shared.Math;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    /// <summary>
    /// this class is a test if the performance of collision detection is better using a tree of bounding spheres
    /// instead of one with bounding boxes. but boxes seem to be the better choice since they are a tighter fit
    /// (just imagine e.g. pillars with bounding spheres...)
    /// </summary>
    class ContactMeshMeshSphere
    {
        private static void CollideLeafLeaf(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, ref Matrix worldTransform1,
            Entity entity2, AlignedBox3TreeNode node2, Vector3[] positions2, ref Matrix worldTransform2,
            ref Contact contact, bool reverse
        )
        {
            Sphere3 sphere1, sphere2;
            node1.BoundingBox.CreateSphere3(ref worldTransform1, out sphere1);
            node2.BoundingBox.CreateSphere3(ref worldTransform2, out sphere2);
            if ((sphere1.Center - sphere2.Center).LengthSquared() > (sphere1.Radius + sphere2.Radius)*(sphere1.Radius+sphere2.Radius))
            {
                return;
            }
            
            // "simple" tri<->tri test...
            for (int i = 0; i < node1.NumTriangles; ++i)
            {
                Triangle3 tri1 = node1.GetTriangle(positions1, i);
                tri1.Vertex0 = Vector3.Transform(tri1.Vertex0, worldTransform1);
                tri1.Vertex1 = Vector3.Transform(tri1.Vertex1, worldTransform1);
                tri1.Vertex2 = Vector3.Transform(tri1.Vertex2, worldTransform1);

                for (int j = 0; j < node2.NumTriangles; ++j)
		        {
                    Triangle3 tri2 = node2.GetTriangle(positions2, j);
                    tri2.Vertex0 = Vector3.Transform(tri2.Vertex0, worldTransform2);
                    tri2.Vertex1 = Vector3.Transform(tri2.Vertex1, worldTransform2);
                    tri2.Vertex2 = Vector3.Transform(tri2.Vertex2, worldTransform2);

                    //System.Console.WriteLine("performing triangle<->triangle test!");

                    bool coplanar;
                    Vector3 isectpt1, isectpt2;
                    if (Intersection.IntersectTriangle3Triangle3(ref tri1, ref tri2, out coplanar, out isectpt1, out isectpt2))
                    {
                        Vector3 point = (isectpt1 + isectpt2) / 2.0f;

                        if (!contact.ContainsPoint(ref point))
                        {
                            Debug.Assert((reverse && contact.EntityA == entity2) || contact.EntityA == entity1);
                            Vector3 normal = reverse ? tri2.Normal : tri1.Normal;
                            contact.AddContactPoint(ref point, ref normal);
                        }
                    }
		        }
	        }
        }

        private static void CollideInnerLeaf(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, ref Matrix worldTransform1,
            Entity entity2, AlignedBox3TreeNode node2, Vector3[] positions2, ref Matrix worldTransform2,
            ref Contact contact, bool reverse
        )
        {
            Sphere3 sphere1, sphere2;
            node1.BoundingBox.CreateSphere3(ref worldTransform1, out sphere1);
            node2.BoundingBox.CreateSphere3(ref worldTransform2, out sphere2);
            if ((sphere1.Center - sphere2.Center).LengthSquared() > (sphere1.Radius + sphere2.Radius) * (sphere1.Radius + sphere2.Radius))
            {
                return;
            }


            // we know already that node2 is a leaf. this makes things simpler...

            if (node1.Left.HasChildren)
            {
                CollideInnerLeaf(
                    entity1, node1.Left, positions1, ref worldTransform1,
                    entity2, node2, positions2, ref worldTransform2,
                    ref contact, reverse
                );
            }
            else
            {
                CollideLeafLeaf(
                    entity1, node1.Left, positions1, ref worldTransform1,
                    entity2, node2, positions2, ref worldTransform2,
                    ref contact, reverse
                );
            }

            if (node1.Right.HasChildren)
            {
                CollideInnerLeaf(
                    entity1, node1.Right, positions1, ref worldTransform1,
                    entity2, node2, positions2, ref worldTransform2,
                    ref contact, reverse
                );
            }
            else
            {
                CollideLeafLeaf(
                    entity1, node1.Right, positions1, ref worldTransform1,
                    entity2, node2, positions2, ref worldTransform2,
                    ref contact, reverse
                );
            }
        }

        private static void CollideInnerInner(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, ref Matrix worldTransform1,
            Entity entity2, AlignedBox3TreeNode node2, Vector3[] positions2, ref Matrix worldTransform2,
            ref Contact contact, bool reverse
        )
        {
            Sphere3 sphere1, sphere2;
            node1.BoundingBox.CreateSphere3(ref worldTransform1, out sphere1);
            node2.BoundingBox.CreateSphere3(ref worldTransform2, out sphere2);
            if ((sphere1.Center - sphere2.Center).LengthSquared() > (sphere1.Radius + sphere2.Radius) * (sphere1.Radius + sphere2.Radius))
            {
                return;
            }

            if (node1.Left.HasChildren)
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Left is innernode, node2.Left is innernode
                    CollideInnerInner(
                        entity1, node1.Left, positions1, ref worldTransform1,
                        entity2, node2.Left, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
                else
                {
                    // node1.Left is innernode, node2.Left is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Left, positions1, ref worldTransform1,
                        entity2, node2.Left, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Left is innernode, node2.Right is innernode
                    CollideInnerInner(
                        entity1, node1.Left, positions1, ref worldTransform1,
                        entity2, node2.Right, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
                else
                {
                    // node1.Left is innernode, node2.Right is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Left, positions1, ref worldTransform1,
                        entity2, node2.Right, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
            }
            else
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Left is leafnode, node2.Left is innernode
                    CollideInnerLeaf(
                        entity2, node2.Left, positions2, ref worldTransform2,
                        entity1, node1.Left, positions1, ref worldTransform1,
                        ref contact, !reverse
                    );
                }
                else
                {
                    // node1.Left is leafnode, node2.Left is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Left, positions1, ref worldTransform1,
                        entity2, node2.Left, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Left is leafnode, node2.Right is innernode
                    CollideInnerLeaf(
                        entity2, node2.Right, positions2, ref worldTransform2,
                        entity1, node1.Left, positions1, ref worldTransform1,
                        ref contact, !reverse
                    );
                }
                else
                {
                    // node1.Left is leafnode, node2.Right is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Left, positions1, ref worldTransform1,
                        entity2, node2.Right, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
            }

            if (node1.Right.HasChildren)
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Right is innernode, node2.Left is innernode
                    CollideInnerInner(
                        entity1, node1.Right, positions1, ref worldTransform1,
                        entity2, node2.Left, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
                else
                {
                    // node1.Right is innernode, node2.Left is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Right, positions1, ref worldTransform1,
                        entity2, node2.Left, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Right is innernode, node2.Right is innernode
                    CollideInnerInner(
                        entity1, node1.Right, positions1, ref worldTransform1,
                        entity2, node2.Right, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
                else
                {
                    // node1.Right is innernode, node2.Right is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Right, positions1, ref worldTransform1,
                        entity2, node2.Right, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
            }
            else
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Right is leafnode, node2.Left is innernode
                    CollideInnerLeaf(
                        entity2, node2.Left, positions2, ref worldTransform2,
                        entity1, node1.Right, positions1, ref worldTransform1,
                        ref contact, !reverse
                    );
                }
                else
                {
                    // node1.Right is leafnode, node2.Left is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Right, positions1, ref worldTransform1,
                        entity2, node2.Left, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Right is leafnode, node2.Right is innernode
                    CollideInnerLeaf(
                        entity2, node2.Right, positions2, ref worldTransform2,
                        entity1, node1.Right, positions1, ref worldTransform1,
                        ref contact, !reverse
                    );
                }
                else
                {
                    // node1.Right is leafnode, node2.Right is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Right, positions1, ref worldTransform1,
                        entity2, node2.Right, positions2, ref worldTransform2,
                        ref contact, reverse
                    );
                }
            }
        }

        public static void Test(
            Entity entity1, object boundingVolume1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
            Entity entity2, object boundingVolume2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
            bool needAllContacts, ref Contact contact
        )
        {
            AlignedBox3Tree tree1 = (AlignedBox3Tree)boundingVolume1;
            AlignedBox3Tree tree2 = (AlignedBox3Tree)boundingVolume2;

            // early exit if they do not intersect...
            Sphere3 sphere1, sphere2;
            tree1.BoundingBox.CreateSphere3(ref worldTransform1, out sphere1);
            tree2.BoundingBox.CreateSphere3(ref worldTransform2, out sphere2);
            if ((sphere1.Center - sphere2.Center).LengthSquared() > (sphere1.Radius + sphere2.Radius) * (sphere1.Radius + sphere2.Radius))
            {
                return;
            }

            if (tree1.Root.HasChildren)
            {
                if (tree2.Root.HasChildren)
                {
                    CollideInnerInner(
                        entity1, tree1.Root, tree1.Positions, ref worldTransform1,
                        entity2, tree2.Root, tree2.Positions, ref worldTransform2,
                        ref contact, false
                    );
                }
                else
                {
                    CollideInnerLeaf(
                        entity1, tree1.Root, tree1.Positions, ref worldTransform1,
                        entity2, tree2.Root, tree2.Positions, ref worldTransform2,
                        ref contact, false
                    );
                }
            }
            else
            {
                if (tree2.Root.HasChildren)
                {
                    CollideInnerLeaf(
                        entity2, tree2.Root, tree2.Positions, ref worldTransform2,
                        entity1, tree1.Root, tree1.Positions, ref worldTransform1,
                        ref contact, true
                    );
                }
                else
                {
                    CollideLeafLeaf(
                        entity1, tree1.Root, tree1.Positions, ref worldTransform1,
                        entity2, tree2.Root, tree2.Positions, ref worldTransform2,
                        ref contact, false
                    );
                }
            }
        }
    }
}
