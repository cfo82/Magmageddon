using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Shared.Math
{
    public partial class Intersection
    {
        public static bool IntersectRay3Triangle3(
            ref Ray3 ray,
            ref Triangle3 triangle,
            out float outT,
            out Vector3 outIsectPt
            )
        {
            // code taken from: http://www.geometrictools.com/

            // compute the offset origin, edges, and normal
            Vector3 kDiff, kEdge1, kEdge2, kNormal;
            kDiff = ray.Origin - triangle.Vertex0;
            Vector3.Subtract(ref triangle.Vertex1, ref triangle.Vertex0, out kEdge1);
            Vector3.Subtract(ref triangle.Vertex2, ref triangle.Vertex1, out kEdge2);
            Vector3.Cross(ref kEdge1, ref kEdge2, out kNormal);

            // Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = line direction,
            // E1 = kEdge1, E2 = kEdge2, N = Cross(E1,E2)) by
            //   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
            //   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
            //   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
            float fDdN = Vector3.Dot(ray.Direction, kNormal);
            float fSign;
            if (fDdN > (1e-06))
            {
                fSign = 1.0f;
            }
            else if (fDdN < -(1e-06))
            {
                fSign = -1.0f;
                fDdN = -fDdN;
            }
            else
            {
                // Line and triangle are parallel, call it a "no intersection"
                // even if the line does intersect.
                outT = 0.0f;
                outIsectPt = Vector3.Zero;
                return false;
            }

            float fDdQxE2 = fSign * Vector3.Dot(ray.Direction, Vector3.Cross(kDiff, kEdge2));
            if (fDdQxE2 >= (double)0.0)
            {
                float fDdE1xQ = fSign * Vector3.Dot(ray.Direction, Vector3.Cross(kEdge1, kDiff));
                if (fDdE1xQ >= (double)0.0)
                {
                    if (fDdQxE2 + fDdE1xQ <= fDdN)
                    {
                        // line intersects triangle
                        float fQdN = -fSign * Vector3.Dot(kDiff, kNormal);
                        float fInv = (1.0f) / fDdN;
                        float t = fQdN * fInv;
                        float v = fDdQxE2 * fInv;
                        float w = fDdE1xQ * fInv;
                        float u = 1.0f - v - w;

                        outT = t;
                        outIsectPt = ray.Origin + t * ray.Direction;

                        return true;
                    }
                    // else: b1+b2 > 1, no intersection
                }
                // else: b2 < 0, no intersection
            }
            // else: b1 < 0, no intersection

            outT = 0.0f;
            outIsectPt = Vector3.Zero;
            return false;
        }
    }
}
