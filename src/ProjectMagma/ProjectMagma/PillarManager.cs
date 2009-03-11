using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma
{
    public class PillarManager : IEnumerable<Entity>
    {
        public PillarManager()
        {
            entities = new List<Entity>();
        }

        public void AddPillar(Entity entity)
        {
            if (!this.entities.Contains(entity))
            {
                this.entities.Add(entity);
            }
        }

        public void RemovePillar(Entity entity)
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

        protected class PillarIterator : IEnumerator<Entity>
        {
            public PillarIterator(PillarManager manager)
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

            private PillarManager manager;
            private int index = 0;
        };

        public IEnumerator<Entity> GetEnumerator()
        {
            return new PillarIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PillarIterator(this);
        }

        #endregion

        private List<Entity> entities;
    }
}
