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
            foreach (XmlElement attributeTemplateNode in attributeTemplatesNode.ChildNodes)
            {
                AttributeTemplateData attributeTemplateData = new AttributeTemplateData();
                attributeTemplateData.name = attributeTemplateNode.GetAttribute("name");
                attributeTemplateData.type = attributeTemplateNode.GetAttribute("type");
                levelData.attributeTemplates.Add(attributeTemplateData);
            }
        }

        private void ProcessAttributesNode(XmlElement attributesNode, EntityData entityData)
        {
            foreach (XmlElement attributeNode in attributesNode.ChildNodes)
            {
                AttributeData attributeData = new AttributeData();
                attributeData.template = attributeNode.GetAttribute("template");
                string value = attributeNode.GetAttribute("value");
                string[] valueArray = value.Split(' ');
                attributeData.values = new float[valueArray.Length];
                for (int i = 0; i < valueArray.Length; ++i)
                {
                    attributeData.values[i] = float.Parse(valueArray[i]);
                }
                entityData.attributes.Add(attributeData);
            }
        }

        private void ProcessEntityNode(XmlElement entityNode, EntityData entityData)
        {
            XmlElement attributesNode = GetChild(entityNode, "Attributes");
            ProcessAttributesNode(attributesNode, entityData);
        }

        private void ProcessDataNode(XmlElement dataNode, LevelData levelData)
        {
            foreach (XmlElement entityNode in dataNode.ChildNodes)
            {
                EntityData entityData = new EntityData();
                entityData.name = entityNode.GetAttribute("name");
                ProcessEntityNode(entityNode, entityData);
                levelData.entities.Add(entityData);
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
