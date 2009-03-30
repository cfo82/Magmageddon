using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Shared.Math.Distance
{
    public partial class SquaredDistance
    {
	    // source:
	    // => Geometric Tools for Computer Graphics, chapter 10.3.2
        // => wildmagic4/libfoundation/distance/wm4distvector3triangle3.*
        public static float Vector3Triangle3(
            ref Vector3 point,
            ref Triangle3 triangle,
            out Vector3 closestPoint
        )
        {
            Vector3 vP = point;
            Vector3 vB = triangle.Vertex0;
            Vector3 ve0 = triangle.Vertex1 - triangle.Vertex0;
            Vector3 ve1 = triangle.Vertex2 - triangle.Vertex0;
            Vector3 vd = vB - vP;

            float a = Vector3.Dot(ve0, ve0);
            float b = Vector3.Dot(ve0, ve1);
            float c = Vector3.Dot(ve1, ve1);
            float d = Vector3.Dot(vd, ve0);
            float e = Vector3.Dot(vd, ve1);
            float f = Vector3.Dot(vd, vd);
            float det = System.Math.Abs(a * c - b * b);
            float s = b * e - c * d;
            float t = b * d - a * e;
            float sqrDistance;

            if (s + t <= det)
            {
                if (s < 0.0f)
                {
                    if (t < 0.0f)  // region 4
                    {
                        if (d < 0.0f)
                        {
                            t = 0.0f;
                            if (-d >= a)
                            {
                                s = 1.0f;
                                sqrDistance = a + (2.0f) * d + f;
                            }
                            else
                            {
                                s = -d / a;
                                sqrDistance = d * s + f;
                            }
                        }
                        else
                        {
                            s = 0.0f;
                            if (e >= 0.0f)
                            {
                                t = 0.0f;
                                sqrDistance = f;
                            }
                            else if (-e >= c)
                            {
                                t = 1.0f;
                                sqrDistance = c + 2.0f * e + f;
                            }
                            else
                            {
                                t = -e / c;
                                sqrDistance = e * t + f;
                            }
                        }
                    }
                    else  // region 3
                    {
                        s = 0.0f;
                        if (e >= 0.0f)
                        {
                            t = 0.0f;
                            sqrDistance = f;
                        }
                        else if (-e >= c)
                        {
                            t = 1.0f;
                            sqrDistance = c + 2.0f * e + f;
                        }
                        else
                        {
                            t = -e / c;
                            sqrDistance = e * t + f;
                        }
                    }
                }
                else if (t < 0.0)  // region 5
                {
                    t = 0.0f;
                    if (d >= 0.0)
                    {
                        s = 0.0f;
                        sqrDistance = f;
                    }
                    else if (-d >= a)
                    {
                        s = 1.0f;
                        sqrDistance = a + 2.0f * d + f;
                    }
                    else
                    {
                        s = -d / a;
                        sqrDistance = d * s + f;
                    }
                }
                else  // region 0
                {
                    // minimum at interior point
                    float invDet = 1.0f / det;
                    s *= invDet;
                    t *= invDet;
                    sqrDistance = s * (a * s + b * t + 2.0f * d) + t * (b * s + c * t + 2.0f * e) + f;
                }
            }
            else
            {
                float tmp0, tmp1, numerator, denominator;

                if (s < 0.0f)  // region 2
                {
                    tmp0 = b + d;
                    tmp1 = c + e;
                    if (tmp1 > tmp0)
                    {
                        numerator = tmp1 - tmp0;
                        denominator = a - 2.0f * b + c;
                        if (numerator >= denominator)
                        {
                            s = 1.0f;
                            t = 0.0f;
                            sqrDistance = a + 2.0f * d + f;
                        }
                        else
                        {
                            s = numerator / denominator;
                            t = 1.0f - s;
                            sqrDistance = s * (a * s + b * t + 2.0f * d) +
                                t * (b * s + c * t + 2.0f * e) + f;
                        }
                    }
                    else
                    {
                        s = 0.0f;
                        if (tmp1 <= 0.0f)
                        {
                            t = 1.0f;
                            sqrDistance = c + 2.0f * e + f;
                        }
                        else if (e >= 0.0f)
                        {
                            t = 0.0f;
                            sqrDistance = f;
                        }
                        else
                        {
                            t = -e / c;
                            sqrDistance = e * t + f;
                        }
                    }
                }
                else if (t < 0.0f)  // region 6
                {
                    tmp0 = b + e;
                    tmp1 = a + d;
                    if (tmp1 > tmp0)
                    {
                        numerator = tmp1 - tmp0;
                        denominator = a - 2.0f * b + c;
                        if (numerator >= denominator)
                        {
                            t = 1.0f;
                            s = 0.0f;
                            sqrDistance = c + 2.0f * e + f;
                        }
                        else
                        {
                            t = numerator / denominator;
                            s = 1.0f - t;
                            sqrDistance = s * (a * s + b * t + 2.0f * d) +
                                t * (b * s + c * t + 2.0f * e) + f;
                        }
                    }
                    else
                    {
                        t = 0.0f;
                        if (tmp1 <= 0.0f)
                        {
                            s = 1.0f;
                            sqrDistance = a + 2.0f * d + f;
                        }
                        else if (d >= 0.0f)
                        {
                            s = 0.0f;
                            sqrDistance = f;
                        }
                        else
                        {
                            s = -d / a;
                            sqrDistance = d * s + f;
                        }
                    }
                }
                else  // region 1
                {
                    numerator = c + e - b - d;
                    if (numerator <= 0.0f)
                    {
                        s = 0.0f;
                        t = 1.0f;
                        sqrDistance = c + 2.0f * e + f;
                    }
                    else
                    {
                        denominator = a - 2.0f * b + c;
                        if (numerator >= denominator)
                        {
                            s = 1.0f;
                            t = 0.0f;
                            sqrDistance = a + 2.0f * d + f;
                        }
                        else
                        {
                            s = numerator / denominator;
                            t = 1.0f - s;
                            sqrDistance = s * (a * s + b * t + 2.0f * d) +
                                t * (b * s + c * t + 2.0f * e) + f;
                        }
                    }
                }
            }

            // account for numerical round-off error
            if (sqrDistance < 0.0f)
            {
                sqrDistance = 0.0f;
            }

            closestPoint = vB + s * ve0 + t * ve1;
            return sqrDistance;
        }
    }

    public partial class Distance
    {
        public static float Vector3Triangle3(
            ref Vector3 point,
            ref Triangle3 triangle,
            out Vector3 closestPoint
        )
        {
            return (float)System.Math.Sqrt(SquaredDistance.Vector3Triangle3(ref point, ref triangle, out closestPoint));
        }
    }
}
