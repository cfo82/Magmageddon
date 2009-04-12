using Microsoft.Xna.Framework;

using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Shared.Math
{
    public partial class Intersection
    {
        public static bool IntersectBox3Box3(
            Box3 box0,
            Box3 box1
        )
        {
            // Cutoff for cosine of angles between box axes.  This is used to catch
            // the cases when at least one pair of axes are parallel.  If this
            // happens, there is no need to test for separation along the
            // Cross(A[i],B[j]) directions.
            float fCutoff = (float)1.0 - 1e-06f;
            bool bExistsParallelPair = false;
            int i;

            // convenience variables
            Vector3[] akA = box0.Axis;
            Vector3[] akB = box1.Axis;
            float[] afEA = box0.HalfDim;
            float[] afEB = box1.HalfDim;

            // compute difference of box centers, D = C1-C0
            Vector3 kD = box1.Center - box0.Center;

            float[,] aafC = new float[3, 3];     // matrix C = A^T B, c_{ij} = Dot(A_i,B_j)
            float[,] aafAbsC = new float[3, 3];  // |c_{ij}|
            float[] afAD = new float[3];        // Dot(A_i,D)
            float fR0, fR1, fR;   // interval radii and distance between centers
            float fR01;           // = R0 + R1

            // axis C0+t*A0
            for (i = 0; i < 3; i++)
            {
                aafC[0, i] = Vector3.Dot(akA[0], akB[i]);
                aafAbsC[0, i] = System.Math.Abs(aafC[0, i]);
                if (aafAbsC[0, i] > fCutoff)
                {
                    bExistsParallelPair = true;
                }
            }
            afAD[0] = Vector3.Dot(akA[0], kD);
            fR = System.Math.Abs(afAD[0]);
            fR1 = afEB[0] * aafAbsC[0, 0] + afEB[1] * aafAbsC[0, 1] + afEB[2] * aafAbsC[0, 2];
            fR01 = afEA[0] + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A1
            for (i = 0; i < 3; i++)
            {
                aafC[1, i] = Vector3.Dot(akA[1], akB[i]);
                aafAbsC[1, i] = System.Math.Abs(aafC[1, i]);
                if (aafAbsC[1, i] > fCutoff)
                {
                    bExistsParallelPair = true;
                }
            }
            afAD[1] = Vector3.Dot(akA[1], kD);
            fR = System.Math.Abs(afAD[1]);
            fR1 = afEB[0] * aafAbsC[1, 0] + afEB[1] * aafAbsC[1, 1] + afEB[2] * aafAbsC[1, 2];
            fR01 = afEA[1] + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A2
            for (i = 0; i < 3; i++)
            {
                aafC[2, i] = Vector3.Dot(akA[2], akB[i]);
                aafAbsC[2, i] = System.Math.Abs(aafC[2, i]);
                if (aafAbsC[2, i] > fCutoff)
                {
                    bExistsParallelPair = true;
                }
            }
            afAD[2] = Vector3.Dot(akA[2], kD);
            fR = System.Math.Abs(afAD[2]);
            fR1 = afEB[0] * aafAbsC[2, 0] + afEB[1] * aafAbsC[2, 1] + afEB[2] * aafAbsC[2, 2];
            fR01 = afEA[2] + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*B0
            fR = System.Math.Abs(Vector3.Dot(akB[0], kD));
            fR0 = afEA[0] * aafAbsC[0, 0] + afEA[1] * aafAbsC[1, 0] + afEA[2] * aafAbsC[2, 0];
            fR01 = fR0 + afEB[0];
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*B1
            fR = System.Math.Abs(Vector3.Dot(akB[1], kD));
            fR0 = afEA[0] * aafAbsC[0, 1] + afEA[1] * aafAbsC[1, 1] + afEA[2] * aafAbsC[2, 1];
            fR01 = fR0 + afEB[1];
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*B2
            fR = System.Math.Abs(Vector3.Dot(akB[2], kD));
            fR0 = afEA[0] * aafAbsC[0, 2] + afEA[1] * aafAbsC[1, 2] + afEA[2] * aafAbsC[2, 2];
            fR01 = fR0 + afEB[2];
            if (fR > fR01)
            {
                return false;
            }

            // At least one pair of box axes was parallel, so the separation is
            // effectively in 2D where checking the "edge" normals is sufficient for
            // the separation of the boxes.
            if (bExistsParallelPair)
            {
                return true;
            }

            // axis C0+t*A0xB0
            fR = System.Math.Abs(afAD[2] * aafC[1, 0] - afAD[1] * aafC[2, 0]);
            fR0 = afEA[1] * aafAbsC[2, 0] + afEA[2] * aafAbsC[1, 0];
            fR1 = afEB[1] * aafAbsC[0, 2] + afEB[2] * aafAbsC[0, 1];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A0xB1
            fR = System.Math.Abs(afAD[2] * aafC[1, 1] - afAD[1] * aafC[2, 1]);
            fR0 = afEA[1] * aafAbsC[2, 1] + afEA[2] * aafAbsC[1, 1];
            fR1 = afEB[0] * aafAbsC[0, 2] + afEB[2] * aafAbsC[0, 0];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A0xB2
            fR = System.Math.Abs(afAD[2] * aafC[1, 2] - afAD[1] * aafC[2, 2]);
            fR0 = afEA[1] * aafAbsC[2, 2] + afEA[2] * aafAbsC[1, 2];
            fR1 = afEB[0] * aafAbsC[0, 1] + afEB[1] * aafAbsC[0, 0];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A1xB0
            fR = System.Math.Abs(afAD[0] * aafC[2, 0] - afAD[2] * aafC[0, 0]);
            fR0 = afEA[0] * aafAbsC[2, 0] + afEA[2] * aafAbsC[0, 0];
            fR1 = afEB[1] * aafAbsC[1, 2] + afEB[2] * aafAbsC[1, 1];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A1xB1
            fR = System.Math.Abs(afAD[0] * aafC[2, 1] - afAD[2] * aafC[0, 1]);
            fR0 = afEA[0] * aafAbsC[2, 1] + afEA[2] * aafAbsC[0, 1];
            fR1 = afEB[0] * aafAbsC[1, 2] + afEB[2] * aafAbsC[1, 0];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A1xB2
            fR = System.Math.Abs(afAD[0] * aafC[2, 2] - afAD[2] * aafC[0, 2]);
            fR0 = afEA[0] * aafAbsC[2, 2] + afEA[2] * aafAbsC[0, 2];
            fR1 = afEB[0] * aafAbsC[1, 1] + afEB[1] * aafAbsC[1, 0];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A2xB0
            fR = System.Math.Abs(afAD[1] * aafC[0, 0] - afAD[0] * aafC[1, 0]);
            fR0 = afEA[0] * aafAbsC[1, 0] + afEA[1] * aafAbsC[0, 0];
            fR1 = afEB[1] * aafAbsC[2, 2] + afEB[2] * aafAbsC[2, 1];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A2xB1
            fR = System.Math.Abs(afAD[1] * aafC[0, 1] - afAD[0] * aafC[1, 1]);
            fR0 = afEA[0] * aafAbsC[1, 1] + afEA[1] * aafAbsC[0, 1];
            fR1 = afEB[0] * aafAbsC[2, 2] + afEB[2] * aafAbsC[2, 0];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            // axis C0+t*A2xB2
            fR = System.Math.Abs(afAD[1] * aafC[0, 2] - afAD[0] * aafC[1, 2]);
            fR0 = afEA[0] * aafAbsC[1, 2] + afEA[1] * aafAbsC[0, 2];
            fR1 = afEB[0] * aafAbsC[2, 1] + afEB[1] * aafAbsC[2, 0];
            fR01 = fR0 + fR1;
            if (fR > fR01)
            {
                return false;
            }

            return true;
        }
    }
}
