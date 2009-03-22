using ProjectMagma.Framework;

namespace ProjectMagma.Collision.CollisionTests
{
    class ContactCylinderSphere
    {
        public static Contact Test(
            Entity entity1, object boundingVolume1,
            Entity entity2, object boundingVolume2
            )
        {
            return ContactSphereCylinder.Test(entity2, boundingVolume2, entity1, boundingVolume1);
        }
    }
}
