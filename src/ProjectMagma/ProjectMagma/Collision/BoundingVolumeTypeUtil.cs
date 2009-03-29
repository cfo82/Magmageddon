using System;

namespace ProjectMagma.Collision
{
    public class BoundingVolumeTypeUtil
    {
        public static int ToNumber(BoundingVolumeType type)
        {
            switch (type)
            {
                case BoundingVolumeType.Cylinder: return 0;
                case BoundingVolumeType.Mesh: return 1;
                case BoundingVolumeType.Sphere: return 2;
                default: throw new Exception("invalid type '" + type + "'");
            }
        }

        public static BoundingVolumeType ToType(int number)
        {
            switch (number)
            {
                case 0: return BoundingVolumeType.Cylinder;
                case 1: return BoundingVolumeType.Mesh;
                case 2: return BoundingVolumeType.Sphere;
                default: throw new Exception("invalid type constant '" + number + "'");
            }
        }
    }
}
