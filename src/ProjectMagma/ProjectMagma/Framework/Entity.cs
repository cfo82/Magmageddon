using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Framework.Attributes;
using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagma.Framework
{
    public class Entity
    {
        public Entity(EntityManager entityManager, string name)
        {
            this.entityManager = entityManager;
            this.name = name;
            this.attributes = new Dictionary<string, Attribute>();
        }

        public void AddAttribute(AttributeData attributeData)
        {
            AttributeTemplateManager attributeTemplateManager = entityManager.Simulation.AttributeTemplateManager;
            AttributeTemplate attributeTemplate = attributeTemplateManager.GetAttributeTemplate(attributeData.template);
            Attribute attribute = attributeTemplate.CreateAttribute();
            attribute.Initialize(attributeData.values);
            this.attributes.Add(attribute.Name, attribute);
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Dictionary<string, Attribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        private EntityManager entityManager;
        private string name;
        private Dictionary<string, Attribute> attributes;
    }
}
