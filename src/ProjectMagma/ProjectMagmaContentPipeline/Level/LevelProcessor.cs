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

        private void ProcessEntityNode(XmlElement entityElement, EntityData entityData)
        {
            entityData.isAbstract = entityElement.HasAttribute("abstract") && (entityElement.GetAttribute("abstract") == "true");
            entityData.name = entityElement.GetAttribute("name");
            entityData.parent = entityElement.HasAttribute("extends") ? entityElement.GetAttribute("extends") : "";

            XmlElement attributesElement = GetChild(entityElement, "Attributes");
            if (attributesElement != null)
            {
                ProcessAttributesNode(attributesElement, entityData);
            }
            XmlElement propertiesElement = GetChild(entityElement, "Properties");
            if (propertiesElement != null)
            {
                ProcessPropertiesNode(propertiesElement, entityData);
            }

        }

        private void ProcessIncludesElement(XmlElement includesElement, LevelData levelData)
        {
            foreach (XmlNode includeNode in includesElement.ChildNodes)
            {
                XmlElement includeElement = includeNode as XmlElement;
                if (includeElement != null)
                {
                    string includefile = includeElement.GetAttribute("name");

                    // open document
                    XmlDocument document = new XmlDocument();
                    document.Load(includefile);
                    LevelData includedData = ProcessLevelData(document);

                    foreach (EntityData entityData in includedData.entities.Values)
                    {
                        levelData.entities.Add(entityData.name, entityData);
                    }
                }
            }
        }

        private LevelData ProcessLevelData(XmlDocument input)
        {
            LevelData levelData = new LevelData();
            
            XmlElement documentRoot = input.DocumentElement;

            XmlElement includesElement = GetChild(documentRoot, "Includes");
            if (includesElement != null)
            {
                ProcessIncludesElement(includesElement, levelData);
            }

            foreach (XmlNode entityNode in documentRoot.ChildNodes)
            {
                XmlElement entityElement = entityNode as XmlElement;
                if (entityElement != null)
                {
                    EntityData entityData = new EntityData();
                    ProcessEntityNode(entityElement, entityData);
                    levelData.entities.Add(entityData.name, entityData);
                }
            }

            return levelData;
        }

        public override LevelData Process(XmlDocument input, ContentProcessorContext context)
        {
            return ProcessLevelData(input);
        }

    }
}
