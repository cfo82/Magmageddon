using Microsoft.Xna.Framework;

using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Shared.Math
{
    public partial class Intersection
    {
        public static bool IntersectBox3Box3(
            Box3 box0, Matrix orientation0,
            Box3 box1, Matrix orientation1
        )
        {
            // Rotante the centers of box0 to box1, relative to box 0
            Vector3 p = box1.Center - box0.Center;
            Vector3 pp = Vector3.Transform(p, Matrix.Transpose(orientation0));

            Vector3 A = box0.HalfDim;
            Vector3 B = box1.HalfDim;

            Matrix R = Matrix.Transpose(orientation0) * orientation1;
            Matrix Q = new Matrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            Q.M11 = System.Math.Abs(R.M11); Q.M12 = System.Math.Abs(R.M12); Q.M13 = System.Math.Abs(R.M13);
            Q.M21 = System.Math.Abs(R.M21); Q.M22 = System.Math.Abs(R.M22); Q.M23 = System.Math.Abs(R.M23);
            Q.M31 = System.Math.Abs(R.M31); Q.M32 = System.Math.Abs(R.M32); Q.M33 = System.Math.Abs(R.M33);

            // Test for the 15 separating axes
            Box3Box3SAT sat = new Box3Box3SAT();

            // Faces of box 0
            if (!sat.Test(pp.X, A.X + B.X * Q.M11 + B.Y * Q.M12 + B.Z * Q.M13, box0.GetAxis(0), 1)) { return false; }
            if (!sat.Test(pp.Y, A.Y + B.X * Q.M21 + B.Y * Q.M22 + B.Z * Q.M23, box0.GetAxis(1), 2)) { return false; }
            if (!sat.Test(pp.Z, A.Z + B.X * Q.M31 + B.Y * Q.M32 + B.Z * Q.M33, box0.GetAxis(2), 3)) { return false; }

            // Faces of box 1
            if (!sat.Test(Vector3.Dot(box1.GetAxis(0), p), B.X + A.X * Q.M11 + A.Y * Q.M21 + A.Z * Q.M31, box1.GetAxis(0), 4)) { return false; }
            if (!sat.Test(Vector3.Dot(box1.GetAxis(1), p), B.Y + A.X * Q.M12 + A.Y * Q.M22 + A.Z * Q.M32, box1.GetAxis(1), 5)) { return false; }
            if (!sat.Test(Vector3.Dot(box1.GetAxis(2), p), B.Z + A.X * Q.M13 + A.Y * Q.M23 + A.Z * Q.M33, box1.GetAxis(2), 6)) { return false; }

            // Cross products (9 cases)
            if (!sat.Test(pp.Z * R.M21 - pp.Y * R.M31, A.Y * Q.M31 + A.Z * Q.M21 + B.Y * Q.M13 + B.Z * Q.M12, 0, -R.M31, R.M21, 7)) { return false; }
            if (!sat.Test(pp.Z * R.M22 - pp.Y * R.M32, A.Y * Q.M32 + A.Z * Q.M22 + B.X * Q.M13 + B.Z * Q.M11, 0, -R.M32, R.M22, 8)) { return false; }
            if (!sat.Test(pp.Z * R.M23 - pp.Y * R.M33, A.Y * Q.M33 + A.Z * Q.M23 + B.X * Q.M12 + B.Y * Q.M11, 0, -R.M33, R.M23, 9)) { return false; }

            if (!sat.Test(pp.X * R.M31 - pp.Z * R.M11, A.X * Q.M31 + A.Z * Q.M11 + B.Y * Q.M23 + B.Z * Q.M22, R.M31, 0, -R.M11, 10)) { return false; }
            if (!sat.Test(pp.X * R.M32 - pp.Z * R.M12, A.X * Q.M32 + A.Z * Q.M12 + B.X * Q.M23 + B.Z * Q.M21, R.M32, 0, -R.M12, 11)) { return false; }
            if (!sat.Test(pp.X * R.M33 - pp.Z * R.M13, A.X * Q.M33 + A.Z * Q.M13 + B.X * Q.M22 + B.Y * Q.M21, R.M33, 0, -R.M13, 12)) { return false; }

            if (!sat.Test(pp.Y * R.M11 - pp.X * R.M21, A.X * Q.M21 + A.Y * Q.M11 + B.Y * Q.M33 + B.Z * Q.M32, -R.M21, R.M11, 0, 13)) { return false; }
            if (!sat.Test(pp.Y * R.M12 - pp.X * R.M22, A.X * Q.M22 + A.Y * Q.M12 + B.X * Q.M33 + B.Z * Q.M31, -R.M22, R.M12, 0, 14)) { return false; }
            if (!sat.Test(pp.Y * R.M13 - pp.X * R.M23, A.X * Q.M23 + A.Y * Q.M13 + B.X * Q.M32 + B.Y * Q.M31, -R.M23, R.M13, 0, 15)) { return false; }

            if (sat.Code == 0) { return false; }

            return true;
        }
    }
}
