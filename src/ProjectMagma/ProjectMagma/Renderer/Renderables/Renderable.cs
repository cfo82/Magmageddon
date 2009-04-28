using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer
{
    public abstract class Renderable
    {
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
