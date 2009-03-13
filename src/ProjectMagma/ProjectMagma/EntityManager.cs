using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Framework;
using ProjectMagma.Shared.Serialization.LevelData;

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
            this.entities.Add(entity.Name, entity);
        }

        public void AddExisting(Entity entity)
        {
            this.entities.Add(entity.Name, entity);
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
                entities.Add(entity.Name, entity);
            }
            foreach (Entity entity in removeDeferred)
            {
                entities.Remove(entity.Name);
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

        Dictionary<string, Entity> entities;
        List<Entity> addDeferred;
        List<Entity> removeDeferred;
    }
}
