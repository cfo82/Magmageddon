﻿using Microsoft.Xna.Framework;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Renderer
{
    public abstract class Renderable : RendererUpdatable
    {
        public virtual void LoadResources(Renderer renderer)
        {
        }

        public virtual void UnloadResources(Renderer renderer)
        {
        }

        public virtual void Update(Renderer renderer)
        {
        }

        public virtual bool NeedsUpdate
        {
            get{ return false; }
        }

        public abstract void Draw(Renderer renderer);

        public virtual void DrawAfterPost(Renderer renderer)
        {
        }

        public abstract RenderMode RenderMode { get; }
        public abstract Vector3 Position { get; }
        public abstract int RenderPriority { get; }
    }
}
