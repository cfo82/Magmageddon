using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Primitives
{
    public struct Box3
    {
        public struct ContainedAxis
        {
            public Vector3 this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return a0;
                        case 1: return a1;
                        case 2: return a2;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0: a0 = value; break;
                        case 1: a1 = value; break;
                        case 2: a2 = value; break;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
            }

            public Vector3 a0;
            public Vector3 a1;
            public Vector3 a2;
        }

        public struct ContainedHalfDim
        {
            public float this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return h0;
                        case 1: return h1;
                        case 2: return h2;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0: h0 = value; break;
                        case 1: h1 = value; break;
                        case 2: h2 = value; break;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
            }

            public float h0;
            public float h1;
            public float h2;
        }

        public ContainedAxis GetAxes()
        {
            return Axis;
        }

        public Vector3 GetAxis(int axis)
        {
            Debug.Assert(axis >= 0 && axis <= 2);
            return Axis[axis];
        }

        public override string ToString()
        {
            return string.Format("center: {0}, axis-0: {1}, axis-1: {2}, axis-2: {3}, halfDim: {{{4}, {5}, {6}}}", Center, Axis[0], Axis[1], Axis[2], HalfDim[0], HalfDim[1], HalfDim[2]);
        }

        public Vector3 Center;
        public ContainedAxis Axis;
        public ContainedHalfDim HalfDim;
    }
}
