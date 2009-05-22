
namespace ProjectMagma.Framework
{
    public delegate void EntityAddedHandler<EntityType>(AbstractEntityManager<EntityType> manager, EntityType entity) 
        where EntityType : AbstractEntity;
}
