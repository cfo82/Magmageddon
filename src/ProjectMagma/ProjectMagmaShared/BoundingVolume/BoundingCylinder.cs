using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.BoundingVolume
{
    public class BoundingCylinder
    {
        public BoundingCylinder()
        {
        }

        public BoundingCylinder(Vector3 c1, Vector3 c2, float radius)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.radius = radius;
        }

        public bool Intersects(BoundingSphere bs)
        {
            // check collision on y axis
            if (bs.Center.Y - bs.Radius < c1.Y && bs.Center.Y + bs.Radius > c2.Y)
            {
                // check collision in xz
                if (Pow2(bs.Center.X - c1.X) + Pow2(bs.Center.Z - c1.Z) < Pow2(bs.Radius + radius))
                    return true;
            }

            return false;
        }

        private float Pow2(float x)
        {
            return x * x;
        }
        
        public Vector3 Top
        {
            get { return c1; }
        }

        public Vector3 Bottom
        {
            get { return c2; }
        }

        public float Radius
        {
            get { return radius; }
        }

        private Vector3 c1;
        private Vector3 c2;
        private float radius;
    }
}
