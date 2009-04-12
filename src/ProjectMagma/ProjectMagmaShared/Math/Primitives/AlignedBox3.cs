using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Primitives
{
    public class AlignedBox3 : Volume
    {
        public AlignedBox3()
        {
            Min = Max = Vector3.Zero;
        }

        public AlignedBox3(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public VolumeType Type
        {
            get { return VolumeType.AlignedBox3; }
        }

        public void CreateSphere3(
            ref Matrix world,
            out Sphere3 sphere
        )
        {
            Vector3 center = (Min + Max) / 2.0f;
            Vector3 worldCenter = Vector3.Transform(center, world);
            Vector3 worldMax = Vector3.Transform(Max, world);
            Vector3 worldExtent = (worldMax - worldCenter);
            sphere = new Sphere3(worldCenter, worldExtent.Length());
        }

        public Box3 CreateBox3(
            ref Matrix world
        )
        {
            Vector3[] corners = new Vector3[] {
                new Vector3(Min.X, Min.Y, Min.Z),
                new Vector3(Max.X, Min.Y, Min.Z),
                new Vector3(Max.X, Max.Y, Min.Z),
                new Vector3(Min.X, Max.Y, Min.Z),

                new Vector3(Min.X, Min.Y, Max.Z),
                new Vector3(Max.X, Min.Y, Max.Z),
                new Vector3(Max.X, Max.Y, Max.Z),
                new Vector3(Min.X, Max.Y, Max.Z)
            };

            for (int i = 0; i < 8; ++i)
            {
                corners[i] = Vector3.Transform(corners[i], world);
            }

            Vector3 xAxis = corners[1] - corners[0];
            Vector3 yAxis = corners[3] - corners[0];
            Vector3 zAxis = corners[4] - corners[0];

            xAxis.Normalize();
            yAxis.Normalize();
            zAxis.Normalize();

            Vector3 center = (corners[0] + corners[6]) / 2.0f;
            Vector3 halfDim = (corners[6] - corners[0]) / 2.0f;

            Box3 box = new Box3();
            box.Center = center;
            box.Axis[0] = xAxis;
            box.Axis[1] = yAxis;
            box.Axis[2] = zAxis;
            box.HalfDim[0] = halfDim.X;
            box.HalfDim[1] = halfDim.Y;
            box.HalfDim[2] = halfDim.Z;
            return box;
        }

        public Vector3 Min;
        public Vector3 Max;
    }
}
