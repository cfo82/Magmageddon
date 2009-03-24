using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma.Collision
{
    public class Contact
    {
        private Entity entityA;
        private Entity entityB;
        private Vector3 position;
        private Vector3 normal;

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

        public void Reverse()
        {
            Entity temp = entityB;
            entityB = entityA;
            entityA = temp;
            normal = -normal;
        }

        public Vector3 Normal
        {
            get { return normal; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public Entity EntityB
        {
            get { return entityB; }
        }

        public Entity EntityA
        {
            get { return entityA; }
        }
    }
}
