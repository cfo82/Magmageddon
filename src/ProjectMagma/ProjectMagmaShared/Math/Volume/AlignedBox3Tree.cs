using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
{
    public class AlignedBox3Tree
    {
        public AlignedBox3Tree()
        {
        }

        public AlignedBox3Tree(Vector3[] positions, UInt16[] indices)
        {
            Debug.Assert(indices.Length % 3 == 0);

            this.positions = new Vector3[positions.Length];
            Array.Copy(positions, this.positions, positions.Length);
            this.indices = new UInt16[indices.Length];
            Array.Copy(indices, this.indices, indices.Length);
            root = new AlignedBox3TreeNode(this.indices.Length / 3, 0, this.indices, this.positions);
        }

        public AlignedBox3TreeNode Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }

        public Vector3[] Positions
        {
            get
            {
                return positions;
            }
            set
            {
                positions = value;
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
                return root.BoundingBox;
            }
        }

        private Vector3[] positions;
        private UInt16[] indices;
        private AlignedBox3TreeNode root;
    }
}
