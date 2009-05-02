using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Primitives
{
    public struct Sphere3 : Volume
    {
        public Sphere3(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public Vector3 Center
        {
            get { return center;  }
            set { center = value; }
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public VolumeType Type
        {
            get { return VolumeType.Sphere3; }
        }

        public void CreateBox3(
            ref Matrix world,
            out Box3 box
        )
        {
            Vector3 worldCenter, worldMax;
            worldCenter = Vector3.Transform(center, world);
            worldMax = Vector3.Transform(center + new Vector3(radius, radius, radius), world);
            Vector3 halfDim = worldMax - worldCenter;

            box = new Box3();
            box.Center = worldCenter;
            box.Axis[0] = Vector3.UnitX;
            box.Axis[1] = Vector3.UnitY;
            box.Axis[2] = Vector3.UnitZ;
            box.HalfDim[0] = halfDim.X;
            box.HalfDim[1] = halfDim.Y;
            box.HalfDim[2] = halfDim.Z;
        }

        private Vector3 center;
        private float radius;
    }
}
