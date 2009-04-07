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

        public override string ToString()
        {
            return string.Format("center: {0}, axis-0: {1}, axis-1: {2}, axis-2: {3}, halfDim: {{{4}, {5}, {6}}}", Center, Axis[0], Axis[1], Axis[2], HalfDim[0], HalfDim[1], HalfDim[2]);
        }

        public Vector3 Center;
        public Vector3[] Axis = new Vector3[3];
        public float[] HalfDim = new float[3];
    }
}
