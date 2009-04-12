using System;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision
{
    public class BoundingVolumeTypeUtil
    {
        public static int ToNumber(VolumeType type)
        {
            switch (type)
            {
                case VolumeType.Cylinder3: return 0;
                case VolumeType.AlignedBox3Tree: return 1;
                case VolumeType.Sphere3: return 2;
                default: throw new Exception("invalid type '" + type + "'");
            }
        }

        public static VolumeType ToType(int number)
        {
            switch (number)
            {
                case 0: return VolumeType.Cylinder3;
                case 1: return VolumeType.AlignedBox3Tree;
                case 2: return VolumeType.Sphere3;
                default: throw new Exception("invalid type constant '" + number + "'");
            }
        }
    }
}
