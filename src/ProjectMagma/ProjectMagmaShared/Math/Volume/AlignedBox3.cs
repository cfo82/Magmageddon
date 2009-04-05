using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Shared.Math.Volume
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

        public Box3 CreateBox3()
        {
            Box3 box = new Box3();
            box.Center = (Min + Max) / 2.0f;
            box.Axis[0] = Vector3.UnitX;
            box.Axis[1] = Vector3.UnitY;
            box.Axis[2] = Vector3.UnitZ;
            box.HalfDim = (Max - Min) / 2.0f;
            return box;
        }

        public Box3 CreateBox3(
            Vector3 translation,
            Matrix rotation,
            Vector3 scale
        )
        {
            Box3 box = new Box3();
            box.Center = (Min + Max) / 2.0f + translation;
            box.Axis[0] = Vector3.Transform(Vector3.UnitX, rotation);
            box.Axis[1] = Vector3.Transform(Vector3.UnitY, rotation);
            box.Axis[2] = Vector3.Transform(Vector3.UnitZ, rotation);
            box.HalfDim = ((Max - Min) / 2.0f);
            box.HalfDim.X *= scale.X;
            box.HalfDim.Y *= scale.Y;
            box.HalfDim.Z *= scale.Z;
            return box;
        }

        public Vector3 Min;
        public Vector3 Max;
    }
}
