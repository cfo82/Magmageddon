using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Shared.LevelData
{
    public class EntityData
    {
        public EntityData()
        {
            this.isAbstract = false;
            this.name = "";
            this.parent = "";
            attributes = new List<AttributeData>();
            properties = new List<PropertyData>();
        }

        public List<AttributeData> CollectAttributes(LevelData levelData)
        {
            List<AttributeData> collectedAttributes = new List<AttributeData>();
            if (this.parent != "")
            {
                if (!levelData.entities.ContainsKey(parent))
                {
                    throw new Exception("unable to find parent");
                }

                collectedAttributes = levelData.entities[parent].CollectAttributes(levelData);
            }

            foreach (AttributeData attribute in attributes)
            {
                bool contained = false;
                for (int i = 0; i < collectedAttributes.Count; ++i)
                {
                    if (collectedAttributes[i].name == attribute.name)
                    {
                        collectedAttributes[i] = attribute;
                        contained = true;
                    }
                }
                if (!contained)
                {
                    collectedAttributes.Add(attribute);
                }
            }

            return collectedAttributes;
        }

        public List<PropertyData> CollectProperties(LevelData levelData)
        {
            List<PropertyData> collectedProperties = new List<PropertyData>();
            if (this.parent != "")
            {
                if (!levelData.entities.ContainsKey(parent))
                {
                    throw new Exception("unable to find parent");
                }

                collectedProperties = levelData.entities[parent].CollectProperties(levelData);
            }

            foreach (PropertyData property in properties)
            {
                bool contained = false;
                for (int i = 0; i < collectedProperties.Count; ++i)
                {
                    if (collectedProperties[i].name == property.name)
                    {
                        collectedProperties[i] = property;
                        contained = true;
                    }
                }
                if (!contained)
                {
                    collectedProperties.Add(property);
                }
            }

            return collectedProperties;
        }

        public bool isAbstract;
        public string name;
        public string parent;
        public List<AttributeData> attributes;
        public List<PropertyData> properties;
    }
}
