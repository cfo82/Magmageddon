using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Primitives
{
    public struct AlignedBox3 : Volume
    {
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

        public void CreateBox3(
            ref Matrix world,
            out Box3 box
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

            box = new Box3();
            box.Center = center;
            box.Axis[0] = xAxis;
            box.Axis[1] = yAxis;
            box.Axis[2] = zAxis;
            box.HalfDim[0] = System.Math.Abs(halfDim.X);
            box.HalfDim[1] = System.Math.Abs(halfDim.Y);
            box.HalfDim[2] = System.Math.Abs(halfDim.Z);
        }

        public override string ToString()
        {
            return string.Format("Min: {0}, Max: {1}, Center: {2}, Extents: {3}", Min, Max, (Max + Min) / 2.0f, (Max - Min) / 2.0f);
        }

        public Vector3 Min;
        public Vector3 Max;
    }
}
