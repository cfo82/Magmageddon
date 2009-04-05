using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
{
    public class Box3
    {

        public Vector3 GetAxis(int axis)
        {
            Debug.Assert(axis >= 0 && axis <= 2);
            return Axis[axis];
        }

        public Vector3 Center;
        public Vector3[] Axis = new Vector3[3];
        public Vector3 HalfDim;
    }
}
