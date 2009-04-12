using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer
{
    public interface Renderable
    {
        void Draw(Renderer renderer, GameTime gameTime);
        RenderMode RenderMode { get; }
        Vector3 Position { get; }
    }
}
