using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Renderer.Interface
{
    public class RendererUpdateQueue
    {
        public RendererUpdateQueue(double timestamp)
        {
            updates = new List<RendererUpdate>();
            this.timestamp = timestamp;
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
