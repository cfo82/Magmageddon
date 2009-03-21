﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Framework;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Shared.LevelData.Serialization;

namespace ProjectMagma
{
    public class EntityManager : IEnumerable<Entity>
    {
        public EntityManager()
        {
            this.entities = new Dictionary<string, Entity>();
            this.addDeferred = new List<Entity>();
            this.removeDeferred = new List<Entity>();
        }

        public void Add(EntityData entityData)
        {
            Entity entity = new Entity(this, entityData.name);
            foreach (AttributeData attributeData in entityData.attributes)
            {
                entity.AddAttribute(attributeData);
            }
            foreach (PropertyData propertyData in entityData.properties)
            {
                entity.AddProperty(propertyData);
            }
            Add(entity);
        }

        public void Add(Entity entity)
        {
            if (!this.entities.ContainsKey(entity.Name))
            {
                this.entities.Add(entity.Name, entity);
                FireEntityAdded(entity);
            }
            else
            {
                throw new Exception("entity with id '" + entity.Name + "' is already registered!");
            }
        }

        public void Remove(Entity entity)
        {
            if (!this.entities.ContainsKey(entity.Name))
            {
                throw new Exception("no entity with id '" + entity.Name + "' is registered!");
            }

            this.entities.Remove(entity.Name);
            FireEntityRemoved(entity);
        }

        public void AddDeferred(Entity entity)
        {
            addDeferred.Add(entity);
        }

        public void RemoveDeferred(Entity entity)
        {
            removeDeferred.Add(entity);
        }

        public void ExecuteDeferred()
        {
            foreach (Entity entity in addDeferred)
            {
                Add(entity);
            }
            foreach (Entity entity in removeDeferred)
            {
                Remove(entity);
            }

            addDeferred.Clear();
            removeDeferred.Clear();
        }

        public IEnumerator<Entity> GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        public Entity this[string name]
        {
            get
            {
                return entities[name];
            }
        }

        public event EntityAddedHandler EntityAdded;
        public event EntityRemovedHandler EntityRemoved;

        private void FireEntityAdded(Entity entity)
        {
            if (this.EntityAdded != null)
            {
                EntityAdded(this, entity);
            }
        }

        private void FireEntityRemoved(Entity entity)
        {
            if (this.EntityRemoved != null)
            {
                EntityRemoved(this, entity);
            }
        }

        Dictionary<string, Entity> entities;
        List<Entity> addDeferred;
        List<Entity> removeDeferred;
    }
}
