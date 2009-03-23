using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using ProjectMagma.Shared.LevelData;
using ProjectMagma.Shared.LevelData.Serialization;
using System.IO;

namespace ProjectMagmaContentPipeline.Level
{
    [ContentProcessor(DisplayName = "Magma - Level Processor")]
    class LevelProcessor : ContentProcessor<XmlDocument, LevelData>
    {
        private static XmlElement GetChild(XmlElement parent, String childTag)
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

        private void ProcessAttributesNode(XmlElement attributesNode, EntityData entityData, ContentProcessorContext context)
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

        private void ProcessPropertiesNode(XmlElement propertiesNode, EntityData entityData, ContentProcessorContext context)
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

        private void ProcessEntityNode(XmlElement entityElement, EntityData entityData, ContentProcessorContext context)
        {
            entityData.isAbstract = entityElement.HasAttribute("abstract") && (entityElement.GetAttribute("abstract") == "true");
            entityData.name = entityElement.GetAttribute("name");
            entityData.parent = entityElement.HasAttribute("extends") ? entityElement.GetAttribute("extends") : "";

            XmlElement attributesElement = GetChild(entityElement, "Attributes");
            if (attributesElement != null)
            {
                ProcessAttributesNode(attributesElement, entityData, context);
            }
            XmlElement propertiesElement = GetChild(entityElement, "Properties");
            if (propertiesElement != null)
            {
                ProcessPropertiesNode(propertiesElement, entityData, context);
            }

        }

        private void ProcessIncludesElement(XmlElement includesElement, LevelData levelData, ContentProcessorContext context)
        {
            foreach (XmlNode includeNode in includesElement.ChildNodes)
            {
                XmlElement includeElement = includeNode as XmlElement;
                if (includeElement != null)
                {
                    string includefile = includeElement.GetAttribute("name");

                    // add dependency
                    context.AddDependency(Path.GetFullPath(includefile));

                    // open document
                    XmlDocument document = new XmlDocument();
                    document.Load(includefile);
                    LevelData includedData = ProcessLevelData(document, context);

                    foreach (EntityData entityData in includedData.entities.Values)
                    {
                        levelData.entities.Add(entityData.name, entityData);
                    }
                }
            }
        }

        private LevelData ProcessLevelData(XmlDocument input, ContentProcessorContext context)
        {
            LevelData levelData = new LevelData();
            
            XmlElement documentRoot = input.DocumentElement;

            XmlElement includesElement = GetChild(documentRoot, "Includes");
            if (includesElement != null)
            {
                ProcessIncludesElement(includesElement, levelData, context);
            }

            foreach (XmlNode entityNode in documentRoot.ChildNodes)
            {
                XmlElement entityElement = entityNode as XmlElement;
                if (entityElement != null)
                {
                    EntityData entityData = new EntityData();
                    ProcessEntityNode(entityElement, entityData, context);
                    levelData.entities.Add(entityData.name, entityData);
                }
            }

            return levelData;
        }

        public override LevelData Process(XmlDocument input, ContentProcessorContext context)
        {
            return ProcessLevelData(input, context);
        }

    }
}
