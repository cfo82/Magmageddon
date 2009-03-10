using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagmaContentPipeline.Level
{
    [ContentProcessor(DisplayName = "Magma - Level Processor")]
    class LevelProcessor : ContentProcessor<XmlDocument, LevelData>
    {
        private XmlElement GetChild(XmlElement parent, String childTag)
        {
            XmlNodeList list = parent.GetElementsByTagName(childTag);
            return list[0] as XmlElement;
        }

        private void ProcessAttributeTemplatesNode(XmlElement attributeTemplatesNode, LevelData levelData)
        {
            foreach (XmlNode attributeTemplateNode in attributeTemplatesNode.ChildNodes)
            {
                XmlElement attributeTemplateElement = attributeTemplateNode as XmlElement;
                if (attributeTemplateElement != null)
                {
                    AttributeTemplateData attributeTemplateData = new AttributeTemplateData();
                    attributeTemplateData.name = attributeTemplateElement.GetAttribute("name");
                    attributeTemplateData.type = attributeTemplateElement.GetAttribute("type");
                    levelData.attributeTemplates.Add(attributeTemplateData);
                }
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

        private void ProcessEntityNode(XmlElement entityNode, EntityData entityData)
        {
            XmlElement attributesNode = GetChild(entityNode, "Attributes");
            ProcessAttributesNode(attributesNode, entityData);
        }

        private void ProcessDataNode(XmlElement dataNode, LevelData levelData)
        {
            foreach (XmlNode entityNode in dataNode.ChildNodes)
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
        }

        public override LevelData Process(XmlDocument input, ContentProcessorContext context)
        {
            LevelData levelData = new LevelData();
            
            XmlElement documentRoot = input.DocumentElement;

            // process the node with the attribute templates
            XmlElement attributeTemplatesNode = GetChild(documentRoot, "AttributeTemplates");
            ProcessAttributeTemplatesNode(attributeTemplatesNode, levelData);

            // process the property templates...
            // TODO

            XmlElement dataNode = GetChild(documentRoot, "LevelData");
            ProcessDataNode(dataNode, levelData);

            return levelData;
        }

    }
}
