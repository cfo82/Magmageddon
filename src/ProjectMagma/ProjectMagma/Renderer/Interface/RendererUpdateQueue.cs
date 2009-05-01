using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Renderer.Interface
{
    public class RendererUpdateQueue
    {
        public RendererUpdateQueue()
        {
            updates = new List<RendererUpdate>();
        }

        public List<RendererUpdate> updates;
    }
}
