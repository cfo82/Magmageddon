using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Framework.Attributes;

using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagma.Framework
{
    public class Simulation
    {
        public Simulation()
        {
            attributeTemplateManager = new AttributeTemplateManager();
            entityManager = new EntityManager(this);
        }

        public void Initialize(ContentManager content, LevelData levelData)
        {
            foreach (AttributeTemplateData attributeTemplateData in levelData.attributeTemplates)
            {
                attributeTemplateManager.AddAttributeTemplate(attributeTemplateData);
            }
            foreach (EntityData entityData in levelData.entities)
            {
                entityManager.AddEntity(content, entityData);
            }
        }

        public AttributeTemplateManager AttributeTemplateManager
        {
            get
            {
                return this.attributeTemplateManager;
            }
        }

        public EntityManager EntityManager
        {
            get
            {
                return entityManager;
            }
        }

        protected AttributeTemplateManager attributeTemplateManager;
        protected EntityManager entityManager;
    }
}
