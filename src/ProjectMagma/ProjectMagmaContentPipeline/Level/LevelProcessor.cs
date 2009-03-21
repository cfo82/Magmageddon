using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Shared.LevelData.Serialization;

namespace ProjectMagmaContentPipeline.Level
{
    [ContentProcessor(DisplayName = "Magma - Level Processor")]
    class LevelProcessor : ContentProcessor<XmlDocument, LevelData>
    {
        private XmlElement GetChild(XmlElement parent, String childTag)
        {
            XmlNodeList list = parent.GetElementsByTagName(childTag);
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                return list[0] as XmlElement;
            }
        }

        private void ProcessAttributesNode(XmlElement attributesNode, EntityData entityData)
        {
            foreach (XmlNode attributeNode in attributesNode.ChildNodes)
            {
                XmlElement attributeElement = attributeNode as XmlElement;
                if (attributeElement != null)
                {
                    AttributeData attributeData = new AttributeData();
                    attributeData.name = attributeElement.GetAttribute("name");
                    attributeData.template = attributeElement.GetAttribute("template");
                    attributeData.value = attributeElement.GetAttribute("value");
                    entityData.attributes.Add(attributeData);
                }
            }
        }

        private void ProcessPropertiesNode(XmlElement propertiesNode, EntityData entityData)
        {
            foreach (XmlNode propertyNode in propertiesNode.ChildNodes)
            {
                XmlElement propertyElement = propertyNode as XmlElement;
                if (propertyElement != null)
                {
                    PropertyData propertyData = new PropertyData();
                    propertyData.name = propertyElement.GetAttribute("name");
                    propertyData.type = propertyElement.GetAttribute("type");
                    entityData.properties.Add(propertyData);
                }
            }
        }

        private void ProcessEntityNode(XmlElement entityNode, EntityData entityData)
        {
            XmlElement attributesNode = GetChild(entityNode, "Attributes");
            if (attributesNode != null)
            {
                ProcessAttributesNode(attributesNode, entityData);
            }
            XmlElement propertiesNode = GetChild(entityNode, "Properties");
            if (propertiesNode != null)
            {
                ProcessPropertiesNode(propertiesNode, entityData);
            }

        }

        public override LevelData Process(XmlDocument input, ContentProcessorContext context)
        {
            LevelData levelData = new LevelData();
            
            XmlElement documentRoot = input.DocumentElement;

            foreach (XmlNode entityNode in documentRoot.ChildNodes)
            {
                XmlElement entityElement = entityNode as XmlElement;
                if (entityElement != null)
                {
                    EntityData entityData = new EntityData();
                    entityData.name = entityElement.GetAttribute("name");
                    ProcessEntityNode(entityElement, entityData);
                    levelData.entities.Add(entityData);
                }
            }

            return levelData;
        }

    }
}
