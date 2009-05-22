using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Framework;

namespace ProjectMagma.Renderer
{
    public class RendererEntityManager : AbstractEntityManager<RendererEntity>
    {
        protected override RendererEntity CreateEntity(string name)
        {
            return new RendererEntity(name);
        }
    }
}
