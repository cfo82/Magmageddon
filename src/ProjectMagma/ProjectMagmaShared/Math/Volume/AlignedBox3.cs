﻿using System;
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

        public VolumeType Type
        {
            get { return VolumeType.AlignedBox3; }
        }

        public Vector3 Min;
        public Vector3 Max;
    }
}