using Microsoft.Xna.Framework;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Renderer
{
    public abstract class Renderable : RendererUpdatable
    {
        public virtual void LoadResources()
        {
        }

        public virtual void UnloadResources()
        {
        }

        public virtual void Update(Renderer renderer, GameTime gameTime)
        {
        }

        public virtual bool NeedsUpdate
        {
            get{ return false; }
        }

        public abstract void Draw(Renderer renderer, GameTime gameTime);

        public abstract RenderMode RenderMode { get; }
        public abstract Vector3 Position { get; set; }
    }
}
