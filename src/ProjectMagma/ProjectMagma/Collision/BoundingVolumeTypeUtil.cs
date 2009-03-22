using System;

namespace ProjectMagma.Collision
{
    public class BoundingVolumeTypeUtil
    {
        public static int ToNumber(BoundingVolumeType type)
        {
            switch (type)
            {
                case BoundingVolumeType.Sphere: return 0;
                case BoundingVolumeType.Cylinder: return 1;
                default: throw new Exception("invalid type '" + type + "'");
            }
        }

        public static BoundingVolumeType ToType(int number)
        {
            switch (number)
            {
                case 0: return BoundingVolumeType.Sphere;
                case 1: return BoundingVolumeType.Cylinder;
                default: throw new Exception("invalid type constant '" + number + "'");
            }
        }
    }
}
