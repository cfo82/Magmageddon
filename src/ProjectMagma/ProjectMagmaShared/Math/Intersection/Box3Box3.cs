using Microsoft.Xna.Framework;

using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Shared.Math
{
    public partial class Intersection
    {
        public struct TwoDimensionalHelper3
        {
            public float this[int i0, int i1]
            {
                get
                {
                    switch (i0)
                    {
                        case 0: return h0[i1];
                        case 1: return h1[i1];
                        case 2: return h2[i1];
                        default: throw new System.Exception("index out of bounds");
                    }
                }
                set
                {
                    switch (i0)
                    {
                        case 0: h0[i1] = value; break;
                        case 1: h1[i1] = value; break;
                        case 2: h2[i1] = value; break;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
            }

            OneDimensionalHelper3 h0;
            OneDimensionalHelper3 h1;
            OneDimensionalHelper3 h2;
        };

        public struct OneDimensionalHelper3
        {
            public float this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return f0;
                        case 1: return f1;
                        case 2: return f2;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
                set
                {
                    switch (index)
                    {
                        case 0: f0 = value; break;
                        case 1: f1 = value; break;
                        case 2: f2 = value; break;
                        default: throw new System.Exception("index out of bounds");
                    }
                }
            }

            float f0;
            float f1;
            float f2;
        };

        public static bool IntersectBox3Box3(
            ref Box3 box0,
            ref Box3 box1
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
            Box3.ContainedAxis akA = box0.Axis;
            Box3.ContainedAxis akB = box1.Axis;
            Box3.ContainedHalfDim afEA = box0.HalfDim;
            Box3.ContainedHalfDim afEB = box1.HalfDim;

            // compute difference of box centers, D = C1-C0
            Vector3 kD = box1.Center - box0.Center;

            TwoDimensionalHelper3 aafC = new TwoDimensionalHelper3();     // matrix C = A^T B, c_{ij} = Dot(A_i,B_j)
            TwoDimensionalHelper3 aafAbsC = new TwoDimensionalHelper3();  // |c_{ij}|
            OneDimensionalHelper3 afAD = new OneDimensionalHelper3();        // Dot(A_i,D)
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
