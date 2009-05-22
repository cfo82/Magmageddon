
namespace ProjectMagma.Framework
{
    public delegate void EntityRemovedHandler<EntityType>(AbstractEntityManager<EntityType> manager, EntityType entity)
        where EntityType : AbstractEntity;
}
