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

        /*public Box3 CreateBox3()
        {
            Box3 box = new Box3();
            box.Center = (Min + Max) / 2.0f;
            box.Axis[0] = Vector3.UnitX;
            box.Axis[1] = Vector3.UnitY;
            box.Axis[2] = Vector3.UnitZ;
            box.HalfDim = (Max - Min) / 2.0f;
            return box;
        }*/

        public Box3 CreateBox3(
            Vector3 translation,
            Matrix rotation,
            Vector3 scale
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

            Matrix world = Matrix.Identity;
            world *= Matrix.CreateScale(scale);
            world *= rotation;
            world *= Matrix.CreateTranslation(translation);

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
