using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectMagma.Framework;

namespace ProjectMagma
{
    public class EntityKindManager : IEnumerable<Entity>
    {
        public EntityKindManager(EntityManager entityManager, string kind)
        {
            this.kind = kind;
            entities = new List<Entity>();
            entityManager.EntityAdded += new EntityAddedHandler(OnEntityAdded);
            entityManager.EntityRemoved += new EntityRemovedHandler(OnEntityRemoved);
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

        private void OnEntityAdded(EntityManager manager, Entity entity)
        {
            if (entity.HasString("kind"))
            {
                if (kind == entity.GetString("kind"))
                {
                    entities.Add(entity);
                }
            }
        }

        private void OnEntityRemoved(EntityManager manager, Entity entity)
        {
            if (entities.Contains(entity))
            {
                entities.Remove(entity);
            }
        }

        #region Implement IEnumerable interface

        private class IceSpikeIterator : IEnumerator<Entity>
        {
            public IceSpikeIterator(EntityKindManager manager)
            {
                this.manager = manager;
                this.index = -1;
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
                index = -1;
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

            private EntityKindManager manager;
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

        private string kind;
        private List<Entity> entities;
    }
}
