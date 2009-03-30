using ProjectMagma.Shared.Math.Volume;
using ProjectMagma.Framework;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Collision
{
    public class CollisionEntity
    {
        public CollisionEntity(Entity entity, CollisionProperty property, BoundingSphere sphere)
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = BoundingVolumeType.Sphere;
            this.volume = sphere;
        }

        public CollisionEntity(Entity entity, CollisionProperty property, Cylinder3 cylinder)
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = BoundingVolumeType.Cylinder;
            this.volume = cylinder;
        }

        public Entity entity;
        public CollisionProperty collisionProperty;
        public BoundingVolumeType volumeType;
        public object volume;
    }
}
