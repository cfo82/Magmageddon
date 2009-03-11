using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma
{
    public class IslandManager
    {
        public IslandManager()
        {
            entities = new List<Entity>();
        }

        public void AddIsland(Entity entity)
        {
            if (!this.entities.Contains(entity))
            {
                this.entities.Add(entity);
            }
        }

        public void RemoveIsland(Entity entity)
        {
            this.entities.Remove(entity);
        }

        public int Count
        {
            get
            {
                return entities.Count;
            }
        }

        public Entity this[int index]
        {
            get
            {
                return entities[index];
            }
        }

        private List<Entity> entities;
    }
}
