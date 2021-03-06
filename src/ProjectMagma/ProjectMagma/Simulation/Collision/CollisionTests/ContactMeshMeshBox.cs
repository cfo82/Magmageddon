﻿using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using ProjectMagma.Shared.Math;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    class ContactMeshMeshBox
    {
        struct Context
        {
            public Entity entity1;
            public Vector3[] positions1;
            public Matrix worldTransform1;

            public Entity entity2;
            public Vector3[] positions2;
            public Matrix worldTransform2;

            public bool needAllContacts;
            public Contact contact;
        }

        private static void CollideLeafLeaf(
            ref Context context, AlignedBox3TreeNode node1, AlignedBox3TreeNode node2
        )
        {
            Box3 box1, box2;
            node1.BoundingBox.CreateBox3(ref context.worldTransform1, out box1);
            node2.BoundingBox.CreateBox3(ref context.worldTransform2, out box2);
            if (Intersection.IntersectBox3Box3(ref box1, ref box2))
            {
                
                // "simple" tri<->tri test...
                for (int i = 0; i < node1.NumTriangles; ++i)
                {
                    Triangle3 tri1 = node1.GetTriangle(context.positions1, i);
                    tri1.Vertex0 = Vector3.Transform(tri1.Vertex0, context.worldTransform1);
                    tri1.Vertex1 = Vector3.Transform(tri1.Vertex1, context.worldTransform1);
                    tri1.Vertex2 = Vector3.Transform(tri1.Vertex2, context.worldTransform1);

                    for (int j = 0; j < node2.NumTriangles; ++j)
                    {
                        Triangle3 tri2 = node2.GetTriangle(context.positions2, j);
                        tri2.Vertex0 = Vector3.Transform(tri2.Vertex0, context.worldTransform2);
                        tri2.Vertex1 = Vector3.Transform(tri2.Vertex1, context.worldTransform2);
                        tri2.Vertex2 = Vector3.Transform(tri2.Vertex2, context.worldTransform2);

                        //System.Console.WriteLine("performing triangle<->triangle test!");

                        bool coplanar;
                        Vector3 isectpt1, isectpt2;
                        if (Intersection.IntersectTriangle3Triangle3(ref tri1, ref tri2, out coplanar, out isectpt1, out isectpt2))
                        {
                            Vector3 point = (isectpt1 + isectpt2) / 2.0f;

                            if (!context.contact.ContainsPoint(ref point))
                            {
                                Debug.Assert(context.contact.EntityA == context.entity1 && context.contact.EntityB == context.entity2);
                                Vector3 normal = tri1.Normal;
                                context.contact.AddContactPoint(ref point, ref normal);
                            }

                            if (!context.needAllContacts)
                            {
                                break;
                            }
                        }
                    }

                    if (!context.needAllContacts && context.contact.Count > 0)
                    {
                        break;
                    }
                }

                /*
//                Contact c = new Contact(context.entity1, context.entity2);

                Vector3 dir = Vector3.Normalize(box2.Center -  box1.Center);
                Vector3 p = box1.Center+box2.Center/2;
                /*
                Vector3 p1 = context.entity1.GetVector3(CommonNames.Position);
                Vector3 p2 = context.entity2.GetVector3(CommonNames.Position);
                Vector3 dir = Vector3.Normalize(p2-p1);
                  Vector3 p = p2+p1/2;
                 /

                context.contact.AddContactPoint(ref p, ref dir);*/
                //                return c;
            }
        }

        private static void CollideInnerLeaf(
            ref Context context, AlignedBox3TreeNode node1, AlignedBox3TreeNode node2
        )
        {
            Box3 box1, box2;
            node1.BoundingBox.CreateBox3(ref context.worldTransform1, out box1);
            node2.BoundingBox.CreateBox3(ref context.worldTransform2, out box2);
            if (Intersection.IntersectBox3Box3(ref box1, ref box2))
            {
                // we know already that node2 is a leaf. this makes things simpler...
                if (node1.Left.HasChildren)
                {
                    CollideInnerLeaf(ref context, node1.Left, node2);
                }
                else
                {
                    CollideLeafLeaf(ref context, node1.Left, node2);
                }

                if (context.needAllContacts || context.contact.Count == 0)
                {
                    if (node1.Right.HasChildren)
                    {
                        CollideInnerLeaf(ref context, node1.Right, node2);
                    }
                    else
                    {
                        CollideLeafLeaf(ref context, node1.Right, node2);
                    }
                }
            }
        }

        private static void CollideLeafInner(
            ref Context context, AlignedBox3TreeNode node1, AlignedBox3TreeNode node2
        )
        {
            Box3 box1, box2;
            node1.BoundingBox.CreateBox3(ref context.worldTransform1, out box1);
            node2.BoundingBox.CreateBox3(ref context.worldTransform2, out box2);
            if (Intersection.IntersectBox3Box3(ref box1, ref box2))
            {
                // we know already that node2 is a leaf. this makes things simpler...
                if (node2.Left.HasChildren)
                {
                    CollideLeafInner(ref context, node1, node2.Left);
                }
                else
                {
                    CollideLeafLeaf(ref context, node1, node2.Left);
                }

                if (context.needAllContacts || context.contact.Count == 0)
                {
                    if (node2.Right.HasChildren)
                    {
                        CollideLeafInner(ref context, node1, node2.Right);
                    }
                    else
                    {
                        CollideLeafLeaf(ref context, node1, node2.Right);
                    }
                }
            }
        }

        private static void CollideInnerInner(
            ref Context context, AlignedBox3TreeNode node1, AlignedBox3TreeNode node2
        )
        {
            Box3 box1, box2;
            node1.BoundingBox.CreateBox3(ref context.worldTransform1, out box1);
            node2.BoundingBox.CreateBox3(ref context.worldTransform2, out box2);
            if (Intersection.IntersectBox3Box3(ref box1, ref box2))
            {
                if (node1.Left.HasChildren)
                {
                    if (node2.Left.HasChildren)
                    {
                        // node1.Left is innernode, node2.Left is innernode
                        CollideInnerInner(ref context, node1.Left, node2.Left);
                    }
                    else
                    {
                        // node1.Left is innernode, node2.Left is leafnode
                        CollideInnerLeaf(ref context, node1.Left, node2.Left);
                    }

                    if (context.needAllContacts || context.contact.Count == 0)
                    {
                        if (node2.Right.HasChildren)
                        {
                            // node1.Left is innernode, node2.Right is innernode
                            CollideInnerInner(ref context, node1.Left, node2.Right);
                        }
                        else
                        {
                            // node1.Left is innernode, node2.Right is leafnode
                            CollideInnerLeaf(ref context, node1.Left, node2.Right);
                        }
                    }
                }
                else
                {
                    if (node2.Left.HasChildren)
                    {
                        // node1.Left is leafnode, node2.Left is innernode
                        CollideLeafInner(ref context, node1.Left, node2.Left);
                    }
                    else
                    {
                        // node1.Left is leafnode, node2.Left is leafnode
                        CollideLeafLeaf(ref context, node1.Left, node2.Left);
                    }

                    if (context.needAllContacts || context.contact.Count == 0)
                    {
                        if (node2.Right.HasChildren)
                        {
                            // node1.Left is leafnode, node2.Right is innernode
                            CollideLeafInner(ref context, node1.Left, node2.Right);
                        }
                        else
                        {
                            // node1.Left is leafnode, node2.Right is leafnode
                            CollideLeafLeaf(ref context, node1.Left, node2.Right);
                        }
                    }
                }

                if (node1.Right.HasChildren)
                {
                    if (node2.Left.HasChildren)
                    {
                        // node1.Right is innernode, node2.Left is innernode
                        CollideInnerInner(ref context, node1.Right, node2.Left);
                    }
                    else
                    {
                        // node1.Right is innernode, node2.Left is leafnode
                        CollideInnerLeaf(ref context, node1.Right, node2.Left);
                    }

                    if (context.needAllContacts || context.contact.Count == 0)
                    {
                        if (node2.Right.HasChildren)
                        {
                            // node1.Right is innernode, node2.Right is innernode
                            CollideInnerInner(ref context, node1.Right, node2.Right);
                        }
                        else
                        {
                            // node1.Right is innernode, node2.Right is leafnode
                            CollideInnerLeaf(ref context, node1.Right, node2.Right);
                        }
                    }
                }
                else
                {
                    if (node2.Left.HasChildren)
                    {
                        // node1.Right is leafnode, node2.Left is innernode
                        CollideLeafInner(ref context, node1.Right, node2.Left);
                    }
                    else
                    {
                        // node1.Right is leafnode, node2.Left is leafnode
                        CollideLeafLeaf(ref context, node1.Right, node2.Left);
                    }

                    if (context.needAllContacts || context.contact.Count == 0)
                    {
                        if (node2.Right.HasChildren)
                        {
                            // node1.Right is leafnode, node2.Right is innernode
                            CollideLeafInner(ref context, node1.Right, node2.Right);
                        }
                        else
                        {
                            // node1.Right is leafnode, node2.Right is leafnode
                            CollideLeafLeaf(ref context, node1.Right, node2.Right);
                        }
                    }
                }
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
                    AlignedBox3Tree tree2 = (AlignedBox3Tree)boundingVolumes2[j];

                    // early exit if they do not intersect...
                    Box3 box1, box2;
                    tree1.BoundingBox.CreateBox3(ref worldTransform1, out box1);
                    tree2.BoundingBox.CreateBox3(ref worldTransform2, out box2);
                    if (!Intersection.IntersectBox3Box3(ref box1, ref box2))
                    {
                        continue;
                    }

                    Context context = new Context();
                    context.entity1 = entity1;
                    context.positions1 = tree1.Positions;
                    context.worldTransform1 = worldTransform1;
                    context.entity2 = entity2;
                    context.positions2 = tree2.Positions;
                    context.worldTransform2 = worldTransform2;
                    context.needAllContacts = needAllContacts;
                    context.contact = contact;

                    if (tree1.Root.HasChildren)
                    {
                        if (tree2.Root.HasChildren)
                        {
                            CollideInnerInner(ref context, tree1.Root, tree2.Root);
                        }
                        else
                        {
                            CollideInnerLeaf(ref context, tree1.Root, tree2.Root);
                        }
                    }
                    else
                    {
                        if (tree2.Root.HasChildren)
                        {
                            CollideLeafInner(ref context, tree1.Root, tree2.Root);
                        }
                        else
                        {
                            CollideLeafLeaf(ref context, tree1.Root, tree2.Root);
                        }
                    }
                }
            }
        }
    }
}
