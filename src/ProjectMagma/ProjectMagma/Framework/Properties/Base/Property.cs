
namespace ProjectMagma.Framework
{
    public interface Property
    {
        void OnAttached(AbstractEntity entity);
        void OnDetached(AbstractEntity entity);
    }
}
