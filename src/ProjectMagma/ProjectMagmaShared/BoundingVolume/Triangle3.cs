using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.BoundingVolume
{
    public struct Triangle3
    {
        public Triangle3(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }

        public Vector3 Vertex0
        {
            get
            {
                return v0;
            }
            set
            {
                v0 = value;
            }
        }

        public Vector3 Vertex1
        {
            get
            {
                return v1;
            }
            set
            {
                v1 = value;
            }
        }

        public Vector3 Vertex2
        {
            get
            {
                return v2;
            }
            set
            {
                v2 = value;
            }
        }

        private Vector3 v0;
        private Vector3 v1;
        private Vector3 v2;
    }
}
