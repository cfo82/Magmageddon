using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagma.Framework
{
    public class EntityManager
    {
        public EntityManager(Simulation simulation)
        {
            this.simulation = simulation;
            this.entities = new Dictionary<string, Entity>();
        }

        public void AddEntity(EntityData entityData)
        {
            Entity entity = new Entity(this, entityData.name);
            foreach (AttributeData attributeData in entityData.attributes)
            {
                entity.AddAttribute(attributeData);
            }
            this.entities.Add(entity.Name, entity);
        }

        public Simulation Simulation
        {
            get
            {
                return simulation;
            }
        }

        public Dictionary<string, Entity> Entities
        {
            get
            {
                return entities;
            }
        }

        Simulation simulation;
        Dictionary<string, Entity> entities;
    }
}
