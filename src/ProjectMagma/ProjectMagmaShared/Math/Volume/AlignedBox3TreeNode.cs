using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
{
    /// <summary>
    /// this node is part of the collision tree of a triangle mesh. both position and index data will be stored inside the tree.
    /// This node only ocntains references to the respective arrays! 
    /// </summary>
    public class AlignedBox3TreeNode
    {
        /// <summary>
        /// needed for serialization...
        /// </summary>
        public AlignedBox3TreeNode()
        {
        }

        /// <summary>
        /// recursively builds the collision tree. this method calculates the bounding box of the node
        /// and if necessary splits the node into two and continues to build the tree within these child
        /// nodes
        /// constructs this node
        /// </summary>
        /// <param name="numTriangles">
        /// Numbers of Triangles stored inside this node or inside its children
        /// </param>
        /// <param name="baseIndex">
        /// Index into the indices array. This is the point where the part of the array that 
        /// is owned by this node starts
        /// </param>
        /// <param name="indices">
        /// reference to the array containing all triangle indices of the model
        /// </param>
        public AlignedBox3TreeNode(int numTriangles, int baseIndex, UInt16[] indices, Vector3[] positions)
        {
            this.numTriangles = numTriangles;
            this.baseIndex = baseIndex;
            this.indices = indices;

            Debug.Assert(numTriangles > 0);
            if (numTriangles == 0)
            {
                return;
            }

            ComputeBoundingBox(positions);

            if (numTriangles < 10)
            {
                return;
            }

            // split along the axis where we get triangles distributed the best...
            int halfNumTriangles = numTriangles / 2;
            int[] numLeft = new int[3] { Split(positions, Axis.AxisX), Split(positions, Axis.AxisY), Split(positions, Axis.AxisZ) };
            int[] diff = new int[3] { halfNumTriangles - numLeft[0], halfNumTriangles - numLeft[1], halfNumTriangles - numLeft[2] };
            int min = 0; // 0 == x-axis, 1 == y-axis, 2 == z-axis
            Axis minAxis = Axis.AxisX;
            if (System.Math.Abs(diff[1]) < System.Math.Abs(diff[min])) { min = 1; minAxis = Axis.AxisY; }
            if (System.Math.Abs(diff[1]) < System.Math.Abs(diff[min])) { min = 1; minAxis = Axis.AxisZ; }
            int splitCount = Split(positions, minAxis);
            if (splitCount == 0 || splitCount == numTriangles)
            {
                // abort if either 0 or all triangles would be positioned inside a single child
                return;
            }

            left = new AlignedBox3TreeNode(splitCount, 0, indices, positions);
            right = new AlignedBox3TreeNode(numTriangles - splitCount, splitCount * 3, indices, positions);
        }

        /// <summary>
        /// calculates the bounding box of this node
        /// </summary>
        /// <param name="positions"></param>
        private void ComputeBoundingBox(Vector3[] positions)
        {
            if (numTriangles == 0)
            {
                // do nothing... we could somehow invalidate the bounding volume here...
            }
            else
            {
                Vector3 min = positions[indices[baseIndex]];
                Vector3 max = positions[indices[baseIndex]];
                for (int i = 1; i < numTriangles * 3; ++i)
                {
                    if (min.X > positions[indices[baseIndex + i]].X) { min.X = positions[indices[baseIndex + i]].X; }
                    if (min.Y > positions[indices[baseIndex + i]].Y) { min.Y = positions[indices[baseIndex + i]].Y; }
                    if (min.Z > positions[indices[baseIndex + i]].Z) { min.Z = positions[indices[baseIndex + i]].Z; }
                    if (max.X < positions[indices[baseIndex + i]].X) { max.X = positions[indices[baseIndex + i]].X; }
                    if (max.Y < positions[indices[baseIndex + i]].Y) { max.Y = positions[indices[baseIndex + i]].Y; }
                    if (max.Z < positions[indices[baseIndex + i]].Z) { max.Z = positions[indices[baseIndex + i]].Z; }
                }
                boundingBox.Min = min;
                boundingBox.Max = max;
            }
        }

        /// <summary>
        /// splits the node along the given axis into half. take care! this method may change the ordering of the
        /// index array based on the split direction!
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int Split(Vector3[] positions, Axis axis)
        {
            Debug.Assert(axis == Axis.AxisX || axis == Axis.AxisY || axis == Axis.AxisZ);
            float splitValue = 0.0f;

            switch (axis)
            {
                case Axis.AxisX:
                    {
                        for (int i = 0; i < numTriangles; ++i)
                        {
                            splitValue += positions[indices[baseIndex + i * 3 + 0]].X;
                            splitValue += positions[indices[baseIndex + i * 3 + 1]].X;
                            splitValue += positions[indices[baseIndex + i * 3 + 2]].X;
                        }
                        break;
                    }

                case Axis.AxisY:
                    {
                        for (int i = 0; i < numTriangles; ++i)
                        {
                            splitValue += positions[indices[baseIndex + i * 3 + 0]].Y;
                            splitValue += positions[indices[baseIndex + i * 3 + 1]].Y;
                            splitValue += positions[indices[baseIndex + i * 3 + 2]].Y;
                        }
                        break;
                    }

                case Axis.AxisZ:
                    {
                        for (int i = 0; i < numTriangles; ++i)
                        {
                            splitValue += positions[indices[baseIndex + i * 3 + 0]].Z;
                            splitValue += positions[indices[baseIndex + i * 3 + 1]].Z;
                            splitValue += positions[indices[baseIndex + i * 3 + 2]].Z;
                        }
                        break;
                    }
            }

            splitValue /= ((float)numTriangles * 3);

            int numLeft = 0;
            for (int i = 0; i < numTriangles; ++i)
            {
                Vector3 center = (positions[indices[baseIndex + i * 3 + 0]] + positions[indices[baseIndex + i * 3 + 1]] + positions[indices[baseIndex + i * 3 + 2]]) / 3.0f;
                if (
                    (axis == Axis.AxisX && center.X < splitValue) ||
                    (axis == Axis.AxisY && center.Y < splitValue) ||
                    (axis == Axis.AxisZ && center.Z < splitValue))
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        UInt16 tmp = indices[baseIndex + i * 3 + j];
                        indices[baseIndex + i * 3 + j] = indices[baseIndex + numLeft * 3 + j];
                        indices[baseIndex + numLeft * 3 + j] = tmp;
                    }
                }
            }
            return numLeft;
        }

        public bool HasChildren
        {
            get
            {
                Debug.Assert((left == null && right == null) || (left != null && right != null));
                return left != null;
            }
        }

        public Triangle3 GetTriangle(Vector3[] positions, int index)
        {
            return new Triangle3(
                positions[indices[baseIndex + index * 3 + 0]],
                positions[indices[baseIndex + index * 3 + 1]],
                positions[indices[baseIndex + index * 3 + 2]]
            );
        }

        public int NumTriangles
        {
            get
            {
                return numTriangles;
            }
            set
            {
                numTriangles = value;
            }
        }

        public int BaseIndex
        {
            get
            {
                return baseIndex;
            }
            set
            {
                baseIndex = value;
            }
        }

        public UInt16[] Indices
        {
            get
            {
                return indices;
            }
            set
            {
                indices = value;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                return boundingBox;
            }
            set
            {
                boundingBox = value;
            }
        }

        public AlignedBox3TreeNode Left
        {
            get
            {
                return left;
            }
            set
            {
                left = value;
            }
        }

        public AlignedBox3TreeNode Right
        {
            get
            {
                return right;
            }
            set
            {
                right = value;
            }
        }

        private int numTriangles;
        private int baseIndex;
        private UInt16[] indices;
        private BoundingBox boundingBox;
        private AlignedBox3TreeNode left;
        private AlignedBox3TreeNode right;
    }
}
