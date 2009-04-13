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
            Vector3 kDiff = ray.Origin - triangle.Vertex0;
            Vector3 kEdge1 = triangle.Vertex1 - triangle.Vertex0;
            Vector3 kEdge2 = triangle.Vertex2 - triangle.Vertex0;
            Vector3 kNormal = Vector3.Cross(kEdge1, kEdge2);

            // Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = ray direction,
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
                // Ray and triangle are parallel, call it a "no intersection"
                // even if the ray does intersect.
                outT = 0.0f;
                outIsectPt = Vector3.Zero;
                return false;
            }

            float fDdQxE2 = fSign * Vector3.Dot(ray.Direction, Vector3.Cross(kDiff, kEdge2));
            if (fDdQxE2 >= (float)0.0)
            {
                float fDdE1xQ = fSign * Vector3.Dot(ray.Direction, Vector3.Cross(kEdge1, kDiff));
                if (fDdE1xQ >= (float)0.0)
                {
                    if (fDdQxE2 + fDdE1xQ <= fDdN)
                    {
                        // line intersects triangle, check if ray does
                        float fQdN = -fSign * Vector3.Dot(kDiff, kNormal);
                        if (fQdN >= (float)0.0)
                        {
                            // ray intersects triangle
                            float fInv = (1.0f) / fDdN;
                            outT = fQdN * fInv;
                            outIsectPt = ray.Origin + outT * ray.Direction;
                            //m_fTriB1 = fDdQxE2*fInv;
                            //m_fTriB2 = fDdE1xQ*fInv;
                            //m_fTriB0 = 1.0f - m_fTriB1 - m_fTriB2;
                            return true;
                        }
                        // else: t < 0, no intersection
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
