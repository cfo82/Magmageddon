using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma.Collision.CollisionTests
{
    public class Contact
    {
        public Entity entityA;
        public Entity entityB;
        public Vector3 position;
        public Vector3 normal;

        public Contact(Entity entityA, Entity entityB)
        {
            this.entityA = entityA;
            this.entityB = entityB;
        }

        public Contact(Entity entityA, Entity entityB, Vector3 position, Vector3 normal)
        {
            this.entityA = entityA;
            this.entityB = entityB;
            this.position = position;
            this.normal = normal;
        }

        public Contact Reverse()
        {
            return new Contact(entityB, entityA, position, -normal);
        }
    }
}
