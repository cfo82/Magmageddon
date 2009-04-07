using System.Collections.Generic;
using Microsoft.Xna.Framework;

using ProjectMagma.Shared.Math;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    class ContactMeshMesh
    {
        private static void CollideLeafLeaf(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, AlignedBox3TreeNode node2, Vector3[] positions2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts, bool reverse
        )
        {
            Box3 box1 = node1.BoundingBox.CreateBox3(translation1, Matrix.CreateFromQuaternion(rotation1), scale1);
            Box3 box2 = node2.BoundingBox.CreateBox3(translation2, Matrix.CreateFromQuaternion(rotation2), scale2);
            if (!Intersection.IntersectBox3Box3(box1, box2))
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

                        bool newContact = true;
                        foreach (Contact c in contacts)
                        { newContact = newContact && c.Position != point; }

                        if (newContact)
                        {
                            Contact c = null;
                            if (reverse)
                            {
                                c = new Contact(entity2, entity1, point, tri2.Normal);
                            }
                            else
                            {
                                c = new Contact(entity1, entity2, point, tri1.Normal);
                            }
                            contacts.Add(c);
                        }
                    }
		        }
	        }
        }

        private static void CollideInnerLeaf(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, AlignedBox3TreeNode node2, Vector3[] positions2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts, bool reverse
        )
        {
            // TODO Box<->box checks
            Box3 box1 = node1.BoundingBox.CreateBox3(translation1, Matrix.CreateFromQuaternion(rotation1), scale1);
            Box3 box2 = node2.BoundingBox.CreateBox3(translation2, Matrix.CreateFromQuaternion(rotation2), scale2);
            if (!Intersection.IntersectBox3Box3(box1, box2))
            {
                return;
            }


            // we know already that node2 is a leaf. this makes things simpler...

            if (node1.Left.HasChildren)
            {
                CollideInnerLeaf(
                    entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, node2, positions2, worldTransform2, translation2, rotation2, scale2,
                    contacts, reverse
                );
            }
            else
            {
                CollideLeafLeaf(
                    entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, node2, positions2, worldTransform2, translation2, rotation2, scale2,
                    contacts, reverse
                );
            }

            if (node1.Right.HasChildren)
            {
                CollideInnerLeaf(
                    entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, node2, positions2, worldTransform2, translation2, rotation2, scale2,
                    contacts, reverse
                );
            }
            else
            {
                CollideLeafLeaf(
                    entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                    entity2, node2, positions2, worldTransform2, translation2, rotation2, scale2,
                    contacts, reverse
                );
            }
        }

        private static void CollideInnerInner(
            Entity entity1, AlignedBox3TreeNode node1, Vector3[] positions1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, AlignedBox3TreeNode node2, Vector3[] positions2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts, bool reverse
        )
        {
            // TODO Box<->box checks
            Box3 box1 = node1.BoundingBox.CreateBox3(translation1, Matrix.CreateFromQuaternion(rotation1), scale1);
            Box3 box2 = node2.BoundingBox.CreateBox3(translation2, Matrix.CreateFromQuaternion(rotation2), scale2);
            if (!Intersection.IntersectBox3Box3(box1, box2))
            {
                return;
            }

            if (node1.Left.HasChildren)
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Left is innernode, node2.Left is innernode
                    CollideInnerInner(
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
                else
                {
                    // node1.Left is innernode, node2.Left is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Left is innernode, node2.Right is innernode
                    CollideInnerInner(
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
                else
                {
                    // node1.Left is innernode, node2.Right is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
            }
            else
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Left is leafnode, node2.Left is innernode
                    CollideInnerLeaf(
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        contacts, !reverse
                    );
                }
                else
                {
                    // node1.Left is leafnode, node2.Left is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Left is leafnode, node2.Right is innernode
                    CollideInnerLeaf(
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        contacts, !reverse
                    );
                }
                else
                {
                    // node1.Left is leafnode, node2.Right is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Left, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
            }

            if (node1.Right.HasChildren)
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Right is innernode, node2.Left is innernode
                    CollideInnerInner(
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
                else
                {
                    // node1.Right is innernode, node2.Left is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Right is innernode, node2.Right is innernode
                    CollideInnerInner(
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
                else
                {
                    // node1.Right is innernode, node2.Right is leafnode
                    CollideInnerLeaf(
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
            }
            else
            {
                if (node2.Left.HasChildren)
                {
                    // node1.Right is leafnode, node2.Left is innernode
                    CollideInnerLeaf(
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        contacts, !reverse
                    );
                }
                else
                {
                    // node1.Right is leafnode, node2.Left is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Left, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }

                if (node2.Right.HasChildren)
                {
                    // node1.Right is leafnode, node2.Right is innernode
                    CollideInnerLeaf(
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        contacts, !reverse
                    );
                }
                else
                {
                    // node1.Right is leafnode, node2.Right is leafnode
                    CollideLeafLeaf(
                        entity1, node1.Right, positions1, worldTransform1, translation1, rotation1, scale1,
                        entity2, node2.Right, positions2, worldTransform2, translation2, rotation2, scale2,
                        contacts, reverse
                    );
                }
            }
        }

        public static void Test(
            Entity entity1, object boundingVolume1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
            Entity entity2, object boundingVolume2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2,
            List<Contact> contacts
            )
        {
            AlignedBox3Tree tree1 = (AlignedBox3Tree)boundingVolume1;
            AlignedBox3Tree tree2 = (AlignedBox3Tree)boundingVolume2;

            // early exit if they do not intersect...
            Box3 box1 = tree1.BoundingBox.CreateBox3(translation1, Matrix.CreateFromQuaternion(rotation1), scale1);
            Box3 box2 = tree2.BoundingBox.CreateBox3(translation2, Matrix.CreateFromQuaternion(rotation2), scale2);
            if (!Intersection.IntersectBox3Box3(box1, box2))
            {
                return;
            }

            if (tree1.Root.HasChildren)
            {
                if (tree2.Root.HasChildren)
                {
                    CollideInnerInner(
                        entity1, tree1.Root, tree1.Positions, worldTransform1, translation1, rotation1, scale1,
                        entity2, tree2.Root, tree2.Positions, worldTransform2, translation2, rotation2, scale2,
                        contacts, false
                    );
                }
                else
                {
                    CollideInnerLeaf(
                        entity1, tree1.Root, tree1.Positions, worldTransform1, translation1, rotation1, scale1,
                        entity2, tree2.Root, tree2.Positions, worldTransform2, translation2, rotation2, scale2,
                        contacts, false
                    );
                }
            }
            else
            {
                if (tree2.Root.HasChildren)
                {
                    CollideInnerLeaf(
                        entity2, tree2.Root, tree2.Positions, worldTransform2, translation2, rotation2, scale2,
                        entity1, tree1.Root, tree1.Positions, worldTransform1, translation1, rotation1, scale1,
                        contacts, true
                    );
                }
                else
                {
                    CollideLeafLeaf(
                        entity1, tree1.Root, tree1.Positions, worldTransform1, translation1, rotation1, scale1,
                        entity2, tree2.Root, tree2.Positions, worldTransform2, translation2, rotation2, scale2,
                        contacts, false
                    );
                }
            }
        }
    }
}
