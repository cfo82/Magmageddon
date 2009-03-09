using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagma.Framework.Attributes
{
    public class AttributeTemplateManager
    {
        public AttributeTemplateManager()
        {
            templates = new Dictionary<string, AttributeTemplate>();
        }

        public void AddAttributeTemplate(AttributeTemplateData attributeTemplateData)
        {
            this.AddAttributeTemplate(attributeTemplateData.name, attributeTemplateData.type);
        }

        public void AddAttributeTemplate(string name, string type)
        {
            Type typeInstance = Type.GetType(type);
            if (typeInstance == null)
            {
                // TODO: ERROR
            }
            else
            {
                templates.Add(name, new AttributeTemplate(name, typeInstance));
            }
        }

        public AttributeTemplate GetAttributeTemplate(string name)
        {
            return templates[name];
        }

        protected Dictionary<string, AttributeTemplate> templates;
    }
}
