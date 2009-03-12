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
        }

        public void AddEntity(ContentManager content, EntityData entityData)
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
    }
}
