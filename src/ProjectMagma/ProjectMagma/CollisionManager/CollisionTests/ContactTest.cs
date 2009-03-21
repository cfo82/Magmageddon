using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Framework;

namespace ProjectMagma.CollisionManager.CollisionTests
{
    public delegate Contact ContactTest(Entity entity1, object BoundingVolume1, Entity entity2, object BoundingVolume2);
}
