using ProjectMagma.Shared.Math.Volume;
using ProjectMagma.Framework;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Collision
{
    public class CollisionEntity
    {
        public CollisionEntity(Entity entity, CollisionProperty property, Sphere3 sphere)
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = VolumeType.Sphere3;
            this.volume = sphere;
        }

        public CollisionEntity(Entity entity, CollisionProperty property, AlignedBox3Tree tree)
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = VolumeType.AlignedBox3Tree;
            this.volume = tree;
        }

        public CollisionEntity(Entity entity, CollisionProperty property, Cylinder3 cylinder)
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = VolumeType.Cylinder3;
            this.volume = cylinder;
        }

        public Entity entity;
        public CollisionProperty collisionProperty;
        public VolumeType volumeType;
        public object volume;
    }
}
