using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma.Collision.CollisionTests
{
    public delegate Contact ContactTest(
        Entity entity1, object BoundingVolume1, Matrix worldTransform1, Vector3 translation1, Quaternion rotation1, Vector3 scale1,
        Entity entity2, object BoundingVolume2, Matrix worldTransform2, Vector3 translation2, Quaternion rotation2, Vector3 scale2);
}
