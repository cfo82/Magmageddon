using ProjectMagma.Shared.Math.Volume;

namespace ProjectMagma.Simulation.Collision
{
    class CollisionEntity
    {
        public CollisionEntity(
            Entity entity,
            CollisionProperty property,
            Sphere3 sphere,
            bool needAllContacts
        )
        :   this(entity, property, VolumeType.Sphere3, sphere, needAllContacts)
        {
        }

        public CollisionEntity(
            Entity entity,
            CollisionProperty property,
            AlignedBox3Tree tree,
            bool needAllContacts
        )
        :   this(entity, property, VolumeType.AlignedBox3Tree, tree, needAllContacts)
        {
        }

        public CollisionEntity(
            Entity entity, 
            CollisionProperty property,
            Cylinder3 cylinder,
            bool needAllContacts
        )
        :   this(entity, property, VolumeType.Cylinder3, cylinder, needAllContacts)
        {
        }

        public CollisionEntity(
            Entity entity,
            CollisionProperty property,
            VolumeType volumeType,
            object volume,
            bool needAllContacts
        )
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = volumeType;
            this.volume = volume;
            this.needAllContacts = needAllContacts;
        }

        public Entity Entity
        {
            get { return entity; }
        }

        public CollisionProperty CollisionProperty
        {
            get { return collisionProperty; }
        }

        public VolumeType VolumeType
        {
            get { return volumeType; }
        }

        public object Volume
        {
            get { return volume; }
        }

        public bool NeedAllContacts
        {
            get { return needAllContacts; }
        }

        private Entity entity;
        private CollisionProperty collisionProperty;
        private VolumeType volumeType;
        private object volume;
        private bool needAllContacts;
    }
}
