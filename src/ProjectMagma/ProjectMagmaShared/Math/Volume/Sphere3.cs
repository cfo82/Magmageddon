using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
{
    public class Sphere3 : Volume
    {
        public Sphere3()
        {
            sphere = new BoundingSphere();
        }

        public Sphere3(Vector3 center, float radius)
        {
            sphere = new BoundingSphere(center, radius);
        }

        public Vector3 Center
        {
            get { return sphere.Center;  }
            set { sphere.Center = value; }
        }

        public float Radius
        {
            get { return sphere.Radius; }
            set { sphere.Radius = value; }
        }

        public VolumeType Type
        {
            get { return VolumeType.Sphere3; }
        }

        BoundingSphere sphere;
    }
}
