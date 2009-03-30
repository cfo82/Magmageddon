/*
 * "Simple" port of C-code. Original code by Tomas Moller (see below!)
 */

/* Triangle/triangle intersection test routine,
 * by Tomas Moller, 1997.
 * See article "A Fast Triangle-Triangle Intersection Test",
 * Journal of Graphics Tools, 2(2), 1997
 * updated: 2001-06-20 (added line of intersection)
 *
 * int tri_tri_intersect(float V0[3],float V1[3],float V2[3],
 *                       float U0[3],float U1[3],float U2[3])
 *
 * parameters: vertices of triangle 1: V0,V1,V2
 *             vertices of triangle 2: U0,U1,U2
 * result    : returns 1 if the triangles intersect, otherwise 0
 *
 * Here is a version withouts divisions (a little faster)
 * int NoDivTriTriIsect(float V0[3],float V1[3],float V2[3],
 *                      float U0[3],float U1[3],float U2[3]);
 * 
 * This version computes the line of intersection as well (if they are not coplanar):
 * int tri_tri_intersect_with_isectline(float V0[3],float V1[3],float V2[3], 
 *				        float U0[3],float U1[3],float U2[3],int *coplanar,
 *				        float isectpt1[3],float isectpt2[3]);
 * coplanar returns whether the tris are coplanar
 * isectpt1, isectpt2 are the endpoints of the line of intersection
 */

using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Shared.Math
{
    partial class Intersection
    {
        #region Triangle<->Triangle intersection implementation 

        private static float EPSILON = 0.000001f;

        private static void Sub(out Vector3 dest, ref Vector3 v1, ref Vector3 v2)
        { dest = new Vector3(); dest.X = v1.X - v2.X; dest.Y = v1.Y - v2.Y; dest.Z = v1.Z - v2.Z; }

        private static void Add(out Vector3 dest, ref Vector3 v1, ref Vector3 v2)
        { dest = new Vector3(); dest.X = v1.X + v2.X; dest.Y = v1.Y + v2.Y; dest.Z = v1.Z + v2.Z; }

        private static void Mult(out Vector3 dest, ref Vector3 v, float factor)
        { dest = new Vector3(); dest.X = factor * v.X; dest.Y = factor * v.Y; dest.Z = factor * v.Z; }

        private static void Cross(out Vector3 dest, ref Vector3 v1, ref Vector3 v2)
        {
            dest = new Vector3(); dest.X = v1.Y * v2.Z - v1.Z * v2.Y;
            dest.Y = v1.Z * v2.X - v1.X * v2.Z;
            dest.Z = v1.X * v2.Y - v1.Y * v2.X;
        }

        private static float Dot(ref Vector3 v1, ref Vector3 v2)
        {
            return (v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z);
        }

        private static void Set(out Vector3 dest, ref Vector3 src)
        { dest = new Vector3(); dest.X = src.X; dest.Y = src.Y; dest.Z = src.Z; }

        private static void Sort(ref float a, ref float b)
        {
             if(a>b)
             {
               float c;
               c=a;
               a=b;
               b=c; 
             }
        }

        private static void Sort2(ref float a, ref float b, out short smallest)
        {
            if (a > b)
            {
                float c;
                c = a;
                a = b;
                b = c;
                smallest = 1;
            }
            else
            {
                smallest = 0;
            }
        }

        private static void Isect2(
            ref Vector3 VTX0, ref Vector3 VTX1, ref Vector3 VTX2,
            float VV0, float VV1, float VV2,
            float D0, float D1, float D2,
            out float isect0, out float isect1, out Vector3 isectpoint0, out Vector3 isectpoint1
            )
        {
            float tmp = D0 / (D0 - D1);
            Vector3 diff = Vector3.Zero;
            isect0 = VV0 + (VV1 - VV0) * tmp;
            Sub(out diff, ref VTX1, ref VTX0);
            Mult(out diff, ref diff, tmp);
            Add(out isectpoint0, ref diff, ref VTX0);
            tmp = D0 / (D0 - D2);
            isect1 = VV0 + (VV2 - VV0) * tmp;
            Sub(out diff, ref VTX2, ref VTX0);
            Mult(out diff, ref diff, tmp);
            Add(out isectpoint1, ref VTX0, ref diff);
        }

        private static bool EdgeEdgeTest(
            ref Vector3 V0,
            ref Vector3 U0, ref Vector3 U1,
            short i0, short i1,
            float Ax, float Ay
        )
        {
            float u0i0 = 0.0f, u1i0 = 0.0f, u0i1 = 0.0f, u1i1 = 0.0f, v0i0 = 0.0f, v0i1 = 0.0f;
            switch (i0)
            {
                case 0: u0i0 = U0.X; u1i0 = U1.X; v0i0 = V0.X; break;
                case 1: u0i0 = U0.Y; u1i0 = U1.Y; v0i0 = V0.Y; break;
                case 2: u0i0 = U0.Z; u1i0 = U1.Z; v0i0 = V0.Z; break;
            }
            switch (i1)
            {
                case 0: u0i1 = U0.X; u1i1 = U1.X; v0i1 = V0.X; break;
                case 1: u0i1 = U0.Y; u1i1 = U1.Y; v0i1 = V0.Y; break;
                case 2: u0i1 = U0.Z; u1i1 = U1.Z; v0i1 = V0.Z; break;
            }
            float Bx = u0i0 - u1i0;
            float By = u0i1 - u1i1;
            float Cx = v0i0 - u0i0;
            float Cy = v0i1 - u0i1;
            float f = Ay * Bx - Ax * By;
            float d = By * Cx - Bx * Cy;
            if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
            {
                float e = Ax * Cy - Ay * Cx;
                if (f > 0)
                {
                    if (e >= 0 && e <= f) return true;
                }
                else
                {
                    if (e <= 0 && e >= f) return true;
                }
            }
            return false;
        }

        private static bool EdgeAgainstTriEdges(
            ref Vector3 V0, ref Vector3 V1,
            ref Vector3 U0, ref Vector3 U1, ref Vector3 U2,
            short i0, short i1
        )
        {
            float v1i0 = 0.0f, v0i0 = 0.0f, v1i1 = 0.0f, v0i1 = 0.0f;
            switch (i0)
            {
                case 0: v1i0 = V1.X; v0i0 = V0.X; break;
                case 1: v1i0 = V1.Y; v0i0 = V0.Y; break;
                case 2: v1i0 = V1.Z; v0i0 = V0.Z; break;
            }
            switch (i1)
            {
                case 0: v1i1 = V1.X; v0i1 = V0.X; break;
                case 1: v1i1 = V1.Y; v0i1 = V0.Y; break;
                case 2: v1i1 = V1.Z; v0i1 = V0.Z; break;
            }
            float Ax = v1i0 - v0i0;
            float Ay = v1i1 - v0i1;
            /* test edge U0,U1 against V0,V1 */
            if (EdgeEdgeTest(ref V0, ref U0, ref U1, i0, i1, Ax, Ay)) { return true; }
            /* test edge U1,U2 against V0,V1 */
            if (EdgeEdgeTest(ref V0, ref U1, ref U2, i0, i1, Ax, Ay)) { return true; }
            /* test edge U2,U1 against V0,V1 */
            if (EdgeEdgeTest(ref V0, ref U2, ref U0, i0, i1, Ax, Ay)) { return true; }
            return false;
        }

        private static bool PointInTri(
            ref Vector3 V0,
            ref Vector3 U0, ref Vector3 U1, ref Vector3 U2,
            short i0, short i1
        )
        {
            float a, b, c, d0, d1, d2;
            float u1i1 = 0.0f, u0i1 = 0.0f, u1i0 = 0.0f, u0i0 = 0.0f, u2i1 = 0.0f, u2i0 = 0.0f, v0i0 = 0.0f, v0i1 = 0.0f;
            switch (i0)
            {
                case 0: u1i0 = U1.X; u0i0 = U0.X; u2i0 = U2.X; v0i0 = V0.X; break;
                case 1: u1i0 = U1.Y; u0i0 = U0.Y; u2i0 = U2.Y; v0i0 = V0.Y; break;
                case 2: u1i0 = U1.Z; u0i0 = U0.Z; u2i0 = U2.Z; v0i0 = V0.Z; break;
            }
            switch (i1)
            {
                case 0: u1i1 = U1.X; u0i1 = U0.X; u2i1 = U2.X; v0i1 = V0.X; break;
                case 1: u1i1 = U1.Y; u0i1 = U0.Y; u2i1 = U2.Y; v0i1 = V0.Y; break;
                case 2: u1i1 = U1.Z; u0i1 = U0.Z; u2i1 = U2.Z; v0i1 = V0.Z; break;
            }
            /* is T1 completly inside T2? */
            /* check if V0 is inside tri(U0,U1,U2) */
            a = u1i1 - u0i1;
            b = -(u1i0 - u0i0);
            c = -a * u0i0 - b * u0i1;
            d0 = a * v0i0 + b * v0i1 + c;

            a = u2i1 - u1i1;
            b = -(u2i0 - u1i0);
            c = -a * u1i0 - b * u1i1;
            d1 = a * v0i0 + b * v0i1 + c;

            a = u0i1 - u2i1;
            b = -(u0i0 - u2i0);
            c = -a * u2i0 - b * u2i1;
            d2 = a * v0i0 + b * v0i1 + c;
            if (d0 * d1 > 0.0)
            {
                if (d0 * d2 > 0.0) { return true; }
            }
            return false;
        }

        private static bool ComputeIntervalsIsectline(
            ref Vector3 VERT0, ref Vector3 VERT1, ref Vector3 VERT2,
            float VV0, float VV1, float VV2,
            float D0, float D1, float D2,
            float D0D1, float D0D2,
            out float isect0, out float isect1, out Vector3 isectpoint0, out Vector3 isectpoint1)
        {
            if (D0D1 > 0.0f)
            {
                /* here we know that D0D2<=0.0 */
                /* that is D0, D1 are on the same side, D2 on the other or on the plane */
                Isect2(ref VERT2, ref VERT0, ref VERT1, VV2, VV0, VV1, D2, D0, D1, out isect0, out isect1, out isectpoint0, out isectpoint1);
            }
            else if (D0D2 > 0.0f)
            {
                /* here we know that d0d1<=0.0 */
                Isect2(ref VERT1, ref VERT0, ref VERT2, VV1, VV0, VV2, D1, D0, D2, out isect0, out isect1, out isectpoint0, out isectpoint1);
            }
            else if (D1 * D2 > 0.0f || D0 != 0.0f)
            {
                /* here we know that d0d1<=0.0 or that D0!=0.0 */
                Isect2(ref VERT0, ref VERT1, ref VERT2, VV0, VV1, VV2, D0, D1, D2, out isect0, out isect1, out isectpoint0, out isectpoint1);
            }
            else if (D1 != 0.0f)
            {
                Isect2(ref VERT1, ref VERT0, ref VERT2, VV1, VV0, VV2, D1, D0, D2, out isect0, out isect1, out isectpoint0, out isectpoint1);
            }
            else if (D2 != 0.0f)
            {
                Isect2(ref VERT2, ref VERT0, ref VERT1, VV2, VV0, VV1, D2, D0, D1, out isect0, out isect1, out isectpoint0, out isectpoint1);
            }
            else
            {
                /* triangles are coplanar */
                isect0 = 0.0f;
                isect1 = 0.0f;
                isectpoint0 = Vector3.Zero;
                isectpoint1 = Vector3.Zero;
                return true;
            }
            return false;
        }


        private static bool CoplanarTriTri(
            ref Vector3 N,
            ref Vector3 V0, ref Vector3 V1, ref Vector3 V2,
            ref Vector3 U0, ref Vector3 U1, ref Vector3 U2
        )
        {
            Vector3 A = Vector3.Zero;
            short i0, i1;
            /* first project onto an axis-aligned plane, that maximizes the area */
            /* of the triangles, compute indices: i0,i1. */
            A.X = System.Math.Abs(N.X);
            A.Y = System.Math.Abs(N.Y);
            A.Z = System.Math.Abs(N.Z);
            if (A.X > A.Y)
            {
                if (A.X > A.Z)
                {
                    i0 = 1;      /* A[0] is greatest */
                    i1 = 2;
                }
                else
                {
                    i0 = 0;      /* A[2] is greatest */
                    i1 = 1;
                }
            }
            else   /* A[0]<=A[1] */
            {
                if (A.Z > A.Y)
                {
                    i0 = 0;      /* A[2] is greatest */
                    i1 = 1;
                }
                else
                {
                    i0 = 0;      /* A[1] is greatest */
                    i1 = 2;
                }
            }

            /* test all edges of triangle 1 against the edges of triangle 2 */
            if (EdgeAgainstTriEdges(ref V0, ref V1, ref U0, ref U1, ref U2, i0, i1)) { return true; }
            if (EdgeAgainstTriEdges(ref V1, ref V2, ref U0, ref U1, ref U2, i0, i1)) { return true; }
            if (EdgeAgainstTriEdges(ref V2, ref V0, ref U0, ref U1, ref U2, i0, i1)) { return true; }

            /* finally, test if tri1 is totally contained in tri2 or vice versa */
            if (PointInTri(ref V0, ref U0, ref U1, ref U2, i0, i1)) { return true; }
            if (PointInTri(ref U0, ref V0, ref V1, ref V2, i0, i1)) { return true; }

            return false;
        }

        private static bool NoDivTriangleTriangleIntersection(
            ref Vector3 V0, ref Vector3 V1, ref Vector3 V2,
            ref Vector3 U0, ref Vector3 U1, ref Vector3 U2
        )
        {
            Vector3 E1, E2;
            Vector3 N1, N2;
            float d1, d2;
            float du0, du1, du2, dv0, dv1, dv2;
            Vector3 D;
            float[] isect1 = new float[2];
            float[] isect2 = new float[2];
            float du0du1, du0du2, dv0dv1, dv0dv2;
            short index;
            float vp0 = 0.0f, vp1 = 0.0f, vp2 = 0.0f;
            float up0 = 0.0f, up1 = 0.0f, up2 = 0.0f;
            float bb, cc, max;
            float a, b, c, x0, x1;
            float d, e, f, y0, y1;
            float xx, yy, xxyy, tmp;

            /* compute plane equation of triangle(V0,V1,V2) */
            Sub(out E1, ref V1, ref V0);
            Sub(out E2, ref V2, ref V0);
            Cross(out N1, ref E1, ref E2);
            d1 = -Dot(ref N1, ref V0);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = Dot(ref N1, ref U0) + d1;
            du1 = Dot(ref N1, ref U1) + d1;
            du2 = Dot(ref N1, ref U2) + d1;

            /* coplanarity robustness check */
            if (System.Math.Abs(du0) < EPSILON) { du0 = 0.0f; }
            if (System.Math.Abs(du1) < EPSILON) { du1 = 0.0f; }
            if (System.Math.Abs(du2) < EPSILON) { du2 = 0.0f; }
            du0du1 = du0 * du1;
            du0du2 = du0 * du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) /* same sign on all of them + not equal 0 ? */
            { return false; }                    /* no intersection occurs */

            /* compute plane of triangle (U0,U1,U2) */
            Sub(out E1, ref U1, ref U0);
            Sub(out E2, ref U2, ref U0);
            Cross(out N2, ref E1, ref E2);
            d2 = -Dot(ref N2, ref U0);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = Dot(ref N2, ref V0) + d2;
            dv1 = Dot(ref N2, ref V1) + d2;
            dv2 = Dot(ref N2, ref V2) + d2;

            if (System.Math.Abs(dv0) < EPSILON) { dv0 = 0.0f; }
            if (System.Math.Abs(dv1) < EPSILON) { dv1 = 0.0f; }
            if (System.Math.Abs(dv2) < EPSILON) { dv2 = 0.0f; }

            dv0dv1 = dv0 * dv1;
            dv0dv2 = dv0 * dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) /* same sign on all of them + not equal 0 ? */
            { return false; }                    /* no intersection occurs */

            /* compute direction of intersection line */
            Cross(out D, ref N1, ref N2);

            /* compute and index to the largest component of D */
            max = System.Math.Abs(D.X);
            index = 0;
            bb = System.Math.Abs(D.Y);
            cc = System.Math.Abs(D.Z);
            if (bb > max) { max = bb; index = 1; }
            if (cc > max) { max = cc; index = 2; }

            /* this is the simplified projection onto L*/
            switch (index)
            {
                case 0:
                    vp0 = V0.X;
                    vp1 = V1.X;
                    vp2 = V2.X;

                    up0 = U0.X;
                    up1 = U1.X;
                    up2 = U2.X;
                    break;
                case 1:
                    vp0 = V0.Y;
                    vp1 = V1.Y;
                    vp2 = V2.Y;

                    up0 = U0.Y;
                    up1 = U1.Y;
                    up2 = U2.Y;
                    break;
                case 2:
                    vp0 = V0.Z;
                    vp1 = V1.Z;
                    vp2 = V2.Z;

                    up0 = U0.Z;
                    up1 = U1.Z;
                    up2 = U2.Z;
                    break;
            }

            /* compute interval for triangle 1 */
            if (dv0dv1 > 0.0f)
            {
                /* here we know that dv0dv2<=0.0 */
                /* that is dv0, dv1 are on the same side, dv2 on the other or on the plane */
                a = vp2; b = (vp0 - vp2) * dv2; c = (vp1 - vp2) * dv2; x0 = dv2 - dv0; x1 = dv2 - dv1;
            }
            else if (dv0dv2 > 0.0f)
            {
                /* here we know that d0d1<=0.0 */
                a = vp1; b = (vp0 - vp1) * dv1; c = (vp2 - vp1) * dv1; x0 = dv1 - dv0; x1 = dv1 - dv2;
            }
            else if (dv1 * dv2 > 0.0f || dv0 != 0.0f)
            {
                /* here we know that d0d1<=0.0 or that dv0!=0.0 */
                a = vp0; b = (vp1 - vp0) * dv0; c = (vp2 - vp0) * dv0; x0 = dv0 - dv1; x1 = dv0 - dv2;
            }
            else if (dv1 != 0.0f)
            {
                a = vp1; b = (vp0 - vp1) * dv1; c = (vp2 - vp1) * dv1; x0 = dv1 - dv0; x1 = dv1 - dv2;
            }
            else if (dv2 != 0.0f)
            {
                a = vp2; b = (vp0 - vp2) * dv2; c = (vp1 - vp2) * dv2; x0 = dv2 - dv0; x1 = dv2 - dv1;
            }
            else
            {
                /* triangles are coplanar */
                return CoplanarTriTri(ref N1, ref V0, ref V1, ref V2, ref U0, ref U1, ref U2);
            }


            /* compute interval for triangle 2 */
            if (du0du1 > 0.0f)
            {
                /* here we know that du0du2<=0.0 */
                /* that is du0, du1 are on the same side, du2 on the other or on the plane */
                d = up2; e = (up0 - up2) * du2; f = (up1 - up2) * du2; y0 = du2 - du0; y1 = du2 - du1;
            }
            else if (du0du2 > 0.0f)
            {
                /* here we know that d0d1<=0.0 */
                d = up1; e = (up0 - up1) * du1; f = (up2 - up1) * du1; y0 = du1 - du0; y1 = du1 - du2;
            }
            else if (du1 * du2 > 0.0f || du0 != 0.0f)
            {
                /* here we know that d0d1<=0.0 or that du0!=0.0 */
                d = up0; e = (up1 - up0) * du0; f = (up2 - up0) * du0; y0 = du0 - du1; y1 = du0 - du2;
            }
            else if (du1 != 0.0f)
            {
                d = up1; e = (up0 - up1) * du1; f = (up2 - up1) * du1; y0 = du1 - du0; y1 = du1 - du2;
            }
            else if (du2 != 0.0f)
            {
                d = up2; e = (up0 - up2) * du2; f = (up1 - up2) * du2; y0 = du2 - du0; y1 = du2 - du1;
            }
            else
            {
                /* triangles are coplanar */
                return CoplanarTriTri(ref N1, ref V0, ref V1, ref V2, ref U0, ref U1, ref U2);
            }


            xx = x0 * x1;
            yy = y0 * y1;
            xxyy = xx * yy;

            tmp = a * xxyy;
            isect1[0] = tmp + b * x1 * yy;
            isect1[1] = tmp + c * x0 * yy;

            tmp = d * xxyy;
            isect2[0] = tmp + e * xx * y1;
            isect2[1] = tmp + f * xx * y0;

            Sort(ref isect1[0], ref isect1[1]);
            Sort(ref isect2[0], ref isect2[1]);

            if (isect1[1] < isect2[0] || isect2[1] < isect1[0])
            { return false; }

            return true;
        }

        private static bool TriangleTriangleIntersectionWithIntersectionLine(
            Vector3 V0, Vector3 V1, Vector3 V2, // triangle 1
            Vector3 U0, Vector3 U1, Vector3 U2, // triangle 2
            out bool coplanar, out Vector3 isectpt1, out Vector3 isectpt2 // output values
        )
        {
            Vector3 E1 = Vector3.Zero;
            Vector3 E2 = Vector3.Zero;
            Vector3 N1 = Vector3.Zero;
            Vector3 N2 = Vector3.Zero;
            float d1, d2;
            float du0, du1, du2, dv0, dv1, dv2;
            Vector3 D = Vector3.Zero;
            float[] isect1 = new float[2];
            float[] isect2 = new float[2];
            Vector3 isectpointA1 = Vector3.Zero;
            Vector3 isectpointA2 = Vector3.Zero;
            Vector3 isectpointB1 = Vector3.Zero;
            Vector3 isectpointB2 = Vector3.Zero;
            float du0du1, du0du2, dv0dv1, dv0dv2;
            short index;
            float vp0 = 0.0f, vp1 = 0.0f, vp2 = 0.0f;
            float up0 = 0.0f, up1 = 0.0f, up2 = 0.0f;
            float b, c, max;
            float[] diff = new float[3];
            short smallest1, smallest2;

            /* compute plane equation of triangle(V0,V1,V2) */
            Sub(out E1, ref V1, ref V0);
            Sub(out E2, ref V2, ref V0);
            Cross(out N1, ref E1, ref E2);
            d1 = -Dot(ref N1, ref V0);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = Dot(ref N1, ref U0) + d1;
            du1 = Dot(ref N1, ref U1) + d1;
            du2 = Dot(ref N1, ref U2) + d1;

            /* coplanarity robustness check */
            if (System.Math.Abs(du0) < EPSILON) { du0 = 0.0f; }
            if (System.Math.Abs(du1) < EPSILON) { du1 = 0.0f; }
            if (System.Math.Abs(du2) < EPSILON) { du2 = 0.0f; }

            du0du1 = du0 * du1;
            du0du2 = du0 * du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) /* same sign on all of them + not equal 0 ? */
            {
                /* no intersection occurs */
                coplanar = false;
                isectpt1 = Vector3.Zero;
                isectpt2 = Vector3.Zero;
                return false;
            }

            /* compute plane of triangle (U0,U1,U2) */
            Sub(out E1, ref U1, ref U0);
            Sub(out E2, ref U2, ref U0);
            Cross(out N2, ref E1, ref E2);
            d2 = -Dot(ref N2, ref U0);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = Dot(ref N2, ref V0) + d2;
            dv1 = Dot(ref N2, ref V1) + d2;
            dv2 = Dot(ref N2, ref V2) + d2;

            if (System.Math.Abs(dv0) < EPSILON) { dv0 = 0.0f; }
            if (System.Math.Abs(dv1) < EPSILON) { dv1 = 0.0f; }
            if (System.Math.Abs(dv2) < EPSILON) { dv2 = 0.0f; }

            dv0dv1 = dv0 * dv1;
            dv0dv2 = dv0 * dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) /* same sign on all of them + not equal 0 ? */
            {
                /* no intersection occurs */
                coplanar = false;
                isectpt1 = Vector3.Zero;
                isectpt2 = Vector3.Zero;
                return false;
            }

            /* compute direction of intersection line */
            Cross(out D, ref N1, ref N2);

            /* compute and index to the largest component of D */
            max = System.Math.Abs(D.X);
            index = 0;
            b = System.Math.Abs(D.Y);
            c = System.Math.Abs(D.Z);
            if (b > max) { max = b; index = 1; }
            if (c > max) { max = c; index = 2; }

            /* this is the simplified projection onto L*/
            switch (index)
            {
                case 0:
                    vp0 = V0.X;
                    vp1 = V1.X;
                    vp2 = V2.X;

                    up0 = U0.X;
                    up1 = U1.X;
                    up2 = U2.X;
                    break;
                case 1:
                    vp0 = V0.Y;
                    vp1 = V1.Y;
                    vp2 = V2.Y;

                    up0 = U0.Y;
                    up1 = U1.Y;
                    up2 = U2.Y;
                    break;
                case 2:
                    vp0 = V0.Z;
                    vp1 = V1.Z;
                    vp2 = V2.Z;

                    up0 = U0.Z;
                    up1 = U1.Z;
                    up2 = U2.Z;
                    break;
            }


            /* compute interval for triangle 1 */
            coplanar = ComputeIntervalsIsectline(ref V0, ref V1, ref V2, vp0, vp1, vp2, dv0, dv1, dv2,
                                 dv0dv1, dv0dv2, out isect1[0], out isect1[1], out isectpointA1, out isectpointA2);
            if (coplanar)
            {
                isectpt1 = Vector3.Zero;
                isectpt2 = Vector3.Zero;
                return CoplanarTriTri(ref N1, ref V0, ref V1, ref V2, ref U0, ref U1, ref U2);
            }


            /* compute interval for triangle 2 */
            ComputeIntervalsIsectline(ref U0, ref U1, ref U2, up0, up1, up2, du0, du1, du2,
                            du0du1, du0du2, out isect2[0], out isect2[1], out isectpointB1, out isectpointB2);

            Sort2(ref isect1[0], ref isect1[1], out smallest1);
            Sort2(ref isect2[0], ref isect2[1], out smallest2);

            if (isect1[1] < isect2[0] || isect2[1] < isect1[0])
            {
                coplanar = false;
                isectpt1 = Vector3.Zero;
                isectpt2 = Vector3.Zero;
                return false;
            }

            /* at this point, we know that the triangles intersect */
            if (isect2[0] < isect1[0])
            {
                if (smallest1 == 0) { Set(out isectpt1, ref isectpointA1); }
                else { Set(out isectpt1, ref isectpointA2); }

                if (isect2[1] < isect1[1])
                {
                    if (smallest2 == 0) { Set(out isectpt2, ref isectpointB2); }
                    else { Set(out isectpt2, ref isectpointB1); }
                }
                else
                {
                    if (smallest1 == 0) { Set(out isectpt2, ref isectpointA2); }
                    else { Set(out isectpt2, ref isectpointA1); }
                }
            }
            else
            {
                if (smallest2 == 0) { Set(out isectpt1, ref isectpointB1); }
                else { Set(out isectpt1, ref isectpointB2); }

                if (isect2[1] > isect1[1])
                {
                    if (smallest1 == 0) { Set(out isectpt2, ref isectpointA2); }
                    else { Set(out isectpt2, ref isectpointA1); }
                }
                else
                {
                    if (smallest2 == 0) { Set(out isectpt2, ref isectpointB2); }
                    else { Set(out isectpt2, ref isectpointB1); }
                }
            }

            return true;
        }

        #endregion

        public static bool IntersectTriangleTriangle(
            ref Triangle3 triangle1,
            ref Triangle3 triangle2
        )
        {
            return NoDivTriangleTriangleIntersection(
                ref triangle1.Vertex0, ref triangle1.Vertex1, ref triangle1.Vertex2,
                ref triangle2.Vertex0, ref triangle2.Vertex1, ref triangle2.Vertex2);
        }

        public static bool IntersectTriangleTriangle(
            ref Triangle3 triangle1,
            ref Triangle3 triangle2,
            out bool coplanar,
            out Vector3 isectpt1, out Vector3 isectpt2
        )
        {
            return TriangleTriangleIntersectionWithIntersectionLine(
                triangle1.Vertex0, triangle1.Vertex1, triangle1.Vertex2,
                triangle2.Vertex0, triangle2.Vertex1, triangle2.Vertex2,
                out coplanar,
                out isectpt1, out isectpt2
            );
        }
    }
}
