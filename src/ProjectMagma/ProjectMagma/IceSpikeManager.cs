using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma
{
    public class IceSpikeManager : IEnumerable<Entity>
    {
        public IceSpikeManager()
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

        private class IceSpikeIterator : IEnumerator<Entity>
        {
            public IceSpikeIterator(IceSpikeManager manager)
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

            private IceSpikeManager manager;
            private int index = 0;
        };

        public IEnumerator<Entity> GetEnumerator()
        {
            return new IceSpikeIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new IceSpikeIterator(this);
        }

        #endregion

        private List<Entity> entities;
    }
}
