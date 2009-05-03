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
            timestamp = 0;
        }

        public void AddUpdate(RendererUpdate update)
        {
            updates.Add(update);
        }

        public int Count
        {
            get { return updates.Count; }
        }

        public RendererUpdate this[int index]
        {
            get { return updates[index]; }
        }

        public double Timestamp
        {
            get { return timestamp; }
        }

        private List<RendererUpdate> updates;
        private double timestamp;
    }
}
