using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Simulation;
using ProjectMagma.Shared.LevelData;
using System.Diagnostics;

namespace ProjectMagma.Simulation
{
    public class EntityManager : IEnumerable<Entity>
    {
        public EntityManager()
        {
            this.entities = new Dictionary<string, Entity>();
            this.addDeferred = new List<Entity>();
            this.removeDeferred = new List<Entity>();
        }

        public void Load(LevelData levelData)
        {
            foreach (EntityData entityData in levelData.entities.Values)
            {
                if (entityData.isAbstract)
                {
                    continue;
                }

                List<AttributeData> attributes = entityData.CollectAttributes(levelData);
                List<PropertyData> properties = entityData.CollectProperties(levelData);

                Entity entity = new Entity(entityData.name);
                foreach (AttributeData attributeData in attributes)
                {
                    entity.AddAttribute(attributeData.name, attributeData.template, attributeData.value);
                }
                foreach (PropertyData propertyData in properties)
                {
                    entity.AddProperty(propertyData.name, propertyData.type);
                }
                Add(entity);
            }
        }

        public void Load(LevelData levelData, String[] bases, Entity[] entities)
        {
            // load normally 
            Load(levelData);

            // create additional entities
            Debug.Assert(bases.Length == entities.Length);
            for (int i = 0; i < bases.Length; i++)
            {
                String baseEntity = bases[i];
                Entity entity = entities[i];

                List<AttributeData> attributes = levelData.entities[baseEntity].CollectAttributes(levelData);
                List<PropertyData> properties = levelData.entities[baseEntity].CollectProperties(levelData);

                foreach (AttributeData attributeData in attributes)
                {
                    if(!entity.HasAttribute(attributeData.name))
                        entity.AddAttribute(attributeData.name, attributeData.template, attributeData.value);
                }
                foreach (PropertyData propertyData in properties)
                {
                    if (!entity.HasProperty(propertyData.name))
                        entity.AddProperty(propertyData.name, propertyData.type);
                }
                Add(entity);
            }
        }

        public void Add(Entity entity)
        {
            if (entity.Name.Length == 0)
            {
                throw new Exception("entity without name added...");
            }

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
            entity.Destroy();
        }

        public void AddDeferred(Entity entity)
        {
            addDeferred.Add(entity);
        }

        public void RemoveDeferred(Entity entity)
        {
            if (!removeDeferred.Contains(entity))
            {
                removeDeferred.Add(entity);
            }
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

        public int Count
        {
            get
            {
                return entities.Count;
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
