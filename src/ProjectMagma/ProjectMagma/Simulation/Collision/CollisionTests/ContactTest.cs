using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation.Collision.CollisionTests
{
    public delegate void ContactTest(
        Entity entity1, object BoundingVolume1, ref Matrix worldTransform1, ref Vector3 translation1, ref Quaternion rotation1, ref Vector3 scale1,
        Entity entity2, object BoundingVolume2, ref Matrix worldTransform2, ref Vector3 translation2, ref Quaternion rotation2, ref Vector3 scale2,
        List<Contact> contacts
    );
}
