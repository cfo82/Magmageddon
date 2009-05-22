using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectMagma.Shared.LevelData;
using System.Diagnostics;

namespace ProjectMagma.Framework
{
    public abstract class AbstractEntityManager<EntityType>
        : IEnumerable<EntityType>
        where EntityType : AbstractEntity
    {
        public AbstractEntityManager()
        {
            this.entities = new Dictionary<string, EntityType>();
            this.addDeferred = new List<EntityType>();
            this.removeDeferred = new List<EntityType>();
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

                EntityType entity = CreateEntity(entityData.name);
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

        public void AddEntities(LevelData levelData, String[] bases, EntityType[] entities)
        {
            Debug.Assert(bases.Length == entities.Length);
            for (int i = 0; i < bases.Length; i++)
            {
                String baseStr = bases[i];
                EntityType entity = entities[i];
                CollectBaseData(entity, baseStr, levelData);
                Add(entity);
            }
        }

        public void Add(EntityType entity, String baseStr, LevelData levelData)
        {
            CollectBaseData(entity, baseStr, levelData);
            Add(entity);
        }

        private void CollectBaseData(EntityType entity, String baseStr, LevelData levelData)
        {
            List<AttributeData> attributes = levelData.entities[baseStr].CollectAttributes(levelData);
            List<PropertyData> properties = levelData.entities[baseStr].CollectProperties(levelData);

            foreach (AttributeData attributeData in attributes)
            {
                if (!entity.HasAttribute(attributeData.name))
                    entity.AddAttribute(attributeData.name, attributeData.template, attributeData.value);
            }
            foreach (PropertyData propertyData in properties)
            {
                if (!entity.HasProperty(propertyData.name))
                    entity.AddProperty(propertyData.name, propertyData.type);
            }
        }

        public void Add(EntityType entity)
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

        public bool ContainsEntity(EntityType entity)
        {
            return this.entities.ContainsKey(entity.Name);
        }

        public void Remove(EntityType entity)
        {
            if (!this.entities.ContainsKey(entity.Name))
            {
                throw new Exception("no entity with id '" + entity.Name + "' is registered!");
            }

            this.entities.Remove(entity.Name);
            FireEntityRemoved(entity);
            entity.Destroy();
        }

        public void AddDeferred(EntityType entity, String baseStr, LevelData levelData)
        {
            CollectBaseData(entity, baseStr, levelData);
            AddDeferred(entity);
        }

        public void AddDeferred(EntityType entity)
        {
            addDeferred.Add(entity);
        }

        public void RemoveDeferred(EntityType entity)
        {
            if (!removeDeferred.Contains(entity))
            {
                removeDeferred.Add(entity);
            }
        }

        public void ExecuteDeferred()
        {
            foreach (EntityType entity in removeDeferred)
            {
                Remove(entity);
            }
            foreach (EntityType entity in addDeferred)
            {
                Add(entity);
            }
            addDeferred.Clear();
            removeDeferred.Clear();
        }

        public void Clear()
        {
            List<EntityType> entityList = new List<EntityType>();
            entityList.AddRange(this.entities.Values);
            for (int i = entityList.Count; i >= 0; --i)
            {
                if (i >= entityList.Count)
                { i = entityList.Count - 1; }
                FireEntityRemoved(entityList[i]);
            }
            for (int i = entityList.Count; i >= 0; --i)
            {
                if (i >= entityList.Count)
                { i = entityList.Count - 1; }
                entityList[i].DetachAll();
            }
            for (int i = entityList.Count; i >= 0; --i)
            {
                if (i >= entityList.Count)
                { i = entityList.Count - 1; }
                entityList[i].Clear();
            }
            entities.Clear();
        }

        public IEnumerator<EntityType> GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entities.Values.GetEnumerator();
        }

        public EntityType this[string name]
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

        private void FireEntityAdded(EntityType entity)
        {
            if (this.EntityAdded != null)
            {
                EntityAdded(this, entity);
            }
        }

        private void FireEntityRemoved(EntityType entity)
        {
            if (this.EntityRemoved != null)
            {
                EntityRemoved(this, entity);
            }
        }

        public abstract EntityType CreateEntity(string name);

        public event EntityAddedHandler<EntityType> EntityAdded;
        public event EntityRemovedHandler<EntityType> EntityRemoved;

        Dictionary<string, EntityType> entities;
        List<EntityType> addDeferred;
        List<EntityType> removeDeferred;
    }
}
