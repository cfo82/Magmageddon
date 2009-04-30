using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision
{
    class CollisionEntity
    {
        public CollisionEntity(
            Entity entity,
            CollisionProperty property,
            Sphere3[] spheres,
            bool needAllContacts
        )
        :   this(entity, property, VolumeType.Sphere3, spheres, needAllContacts)
        {
        }

        public CollisionEntity(
            Entity entity,
            CollisionProperty property,
            AlignedBox3Tree[] trees,
            bool needAllContacts
        )
        :   this(entity, property, VolumeType.AlignedBox3Tree, trees, needAllContacts)
        {
        }

        public CollisionEntity(
            Entity entity, 
            CollisionProperty property,
            Cylinder3[] cylinders,
            bool needAllContacts
        )
        :   this(entity, property, VolumeType.Cylinder3, cylinders, needAllContacts)
        {
        }

        public CollisionEntity(
            Entity entity,
            CollisionProperty property,
            VolumeType volumeType,
            object[] volumes,
            bool needAllContacts
        )
        {
            this.entity = entity;
            this.collisionProperty = property;
            this.volumeType = volumeType;
            this.volumes = volumes;
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

        public object[] Volumes
        {
            get { return volumes; }
        }

        public bool NeedAllContacts
        {
            get { return needAllContacts; }
        }

        private Entity entity;
        private CollisionProperty collisionProperty;
        private VolumeType volumeType;
        private object[] volumes;
        private bool needAllContacts;
    }
}
