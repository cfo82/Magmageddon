using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma
{
    public class IslandManager : IEnumerable<Entity>
    {
        public IslandManager()
        {
            entities = new List<Entity>();
        }

        public void Add(Entity entity)
        {
            if (!this.entities.Contains(entity))
            {
                this.entities.Add(entity);
            }
        }

        public void Remove(Entity entity)
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

        #region Implement IEnumerable interface

        private class IslandIterator : IEnumerator<Entity>
        {
            public IslandIterator(IslandManager manager)
            {
                this.manager = manager;
                this.index = 0;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ++index;
                return index < manager.Count;
            }

            public void Reset()
            {
                index = 0;
            }

            public Entity Current
            {
                get
                {
                    return manager[index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return manager[index];
                }
            }

            private IslandManager manager;
            private int index = 0;
        };

        public IEnumerator<Entity> GetEnumerator()
        {
            return new IslandIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new IslandIterator(this);
        }

        #endregion

        private List<Entity> entities;
    }
}
