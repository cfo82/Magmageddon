using System;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math
{
    public partial class Intersection
    {
        /// <summary>
        /// got this BoxBoxSAT test from basil fierz. according to what he says it is
        /// a port of some other c-code.
        /// </summary>
        private class Box3Box3SAT
        {
            public bool Test(float expr1, float expr2, Vector3 normal, int c)
            {
                float s2 = System.Math.Abs(expr1) - expr2;
                if (s2 > 0.0f) return false;
                if (s2 > S)
                {
                    S = s2;
                    Normal = normal;
                    TransformNormal = false;
                    InvertNormal = (expr1 < 0.0f);
                    Code = c;
                }

                return true;
            }

            public bool Test(float expr1, float expr2, float n1, float n2, float n3, int c)
            {
                float s2 = System.Math.Abs(expr1) - expr2;
                if (s2 > 0.0f) return false;
                float l = (float)System.Math.Sqrt(n1 * n1 + n2 * n2 + n3 * n3);
                if (l > 0)
                {
                    s2 /= l;
                    if (s2 * 1.05 > S)
                    {
                        S = s2;
                        Normal.X = n1 / l;
                        Normal.Y = n2 / l;
                        Normal.Z = n3 / l;
                        TransformNormal = true;
                        InvertNormal = (expr1 < 0.0f);
                        Code = c;
                    }
                }

                return true;
            }

            public float S = Single.NegativeInfinity;
            public Vector3 Normal = new Vector3();
            public bool TransformNormal = false;
            public bool InvertNormal = false;
            public int Code = 0;
        }
    }
}
