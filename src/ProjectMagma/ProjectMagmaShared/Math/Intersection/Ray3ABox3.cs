using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Shared.Math.Primitives;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math
{
    public partial class Intersection
    {
        //-----------------------------------------------------------------------------------------------------------
        public static bool IntersectRay3AlignedBox3(
            ref Ray3 ray,
            ref AlignedBox3 box
        )
        {

            float t_near = float.MinValue;
            float t_far = float.MaxValue;
            float t0, t1;
            Vector3 min = box.Min;
            Vector3 max = box.Max;

            // check for ray parallel to planes
            if (System.Math.Abs(ray.Direction.X) < 1e-06)
            {
                if (ray.Origin.X < min.X || ray.Origin.X > max.X)
                    return false;
            }
            if (System.Math.Abs(ray.Direction.Y) < 1e-06)
            {
                if (ray.Origin.Y < min.Y || ray.Origin.Y > max.Y)
                    return false;
            }
            if (System.Math.Abs(ray.Direction.Z) < 1e-06)
            {
                if (ray.Origin.Z < min.Z || ray.Origin.Z > max.Z)
                    return false;
            }

            // ray not parallel to planes, so find parameters of intersections for the Y/Z-Plane
            float divx = 1 / ray.Direction.X;
            t0 = (min.X - ray.Origin.X) * divx;
            t1 = (max.X - ray.Origin.X) * divx;
            if (t0 > t1)
            {
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }
            if (t0 > t_near) t_near = t0;
            if (t1 < t_far) t_far = t1;
            if (t_near > t_far) return false;
            if (t_far < 0) return false;

            // for the X/Z-Plane
            float divy = 1 / ray.Direction.Y;
            t0 = (min.Y - ray.Origin.Y) * divy;
            t1 = (max.Y - ray.Origin.Y) * divy;
            if (t0 > t1)
            {
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }
            if (t0 > t_near) t_near = t0;
            if (t1 < t_far) t_far = t1;
            if (t_near > t_far) return false;
            if (t_far < 0) return false;

            // for the X/Y-Plane
            float divz = 1 / ray.Direction.Z;
            t0 = (min.Z - ray.Origin.Z) * divz;
            t1 = (max.Z - ray.Origin.Z) * divz;
            if (t0 > t1)
            {
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }
            if (t0 > t_near) t_near = t0;
            if (t1 < t_far) t_far = t1;
            if (t_near > t_far) return false;
            if (t_far < 0) return false;

            // if we reach this point we've got an intersection
            return true;
        }
    }
}
