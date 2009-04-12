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

        public Box3 CreateBox3(
            ref Matrix world
        )
        {
            Vector3 worldCenter, worldMax;
            worldCenter = Vector3.Transform(sphere.Center, world);
            worldMax = Vector3.Transform(sphere.Center + new Vector3(sphere.Radius, sphere.Radius, sphere.Radius), world);
            Vector3 halfDim = worldMax - worldCenter;

            Box3 box = new Box3();
            box.Center = worldCenter;
            box.Axis[0] = Vector3.UnitX;
            box.Axis[1] = Vector3.UnitY;
            box.Axis[2] = Vector3.UnitZ;
            box.HalfDim[0] = halfDim.X;
            box.HalfDim[1] = halfDim.Y;
            box.HalfDim[2] = halfDim.Z;
            return box;
        }

        BoundingSphere sphere;
    }
}
