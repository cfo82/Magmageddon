using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Simulation.Collision
{
    class CollisionEntity
    {
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
