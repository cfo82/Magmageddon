using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectMagma.Simulation.Attributes;
using ProjectMagma.Renderer;

namespace ProjectMagma.Simulation
{
    public class HUDProperty : Property
    {
        public HUDProperty()
        {
        }

        public void OnAttached(Entity entity)
        {
            Debug.Assert(entity.HasAttribute("kind") && entity.GetString("kind") == "player");
            renderable = new HUDRenderable(entity);
            Game.Instance.Renderer.AddRenderable(renderable);
        }

        public void OnDetached(Entity entity)
        {
            Game.Instance.Renderer.RemoveRenderable(renderable);
        }

        private HUDRenderable renderable;
    }
}
