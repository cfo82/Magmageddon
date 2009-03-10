﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Shared.Serialization.LevelData
{
    public class AttributeTemplateData
    {
        public AttributeTemplateData()
        {
            name = "";
            type = "";
        }

        public string name;
        public string type;
    }

    public class AttributeData
    {
        public AttributeData()
        {
            name = "";
            template = "";
            value = "";
        }

        public string name;
        public string template;
        public string value;
    }

    public class EntityData
    {
        public EntityData()
        {
            name = "";
            attributes = new List<AttributeData>();
        }

        public string name;
        public List<AttributeData> attributes;
    }

    public class LevelData
    {
        public LevelData()
        {
            attributeTemplates = new List<AttributeTemplateData>();
            entities = new List<EntityData>();
        }

        public List<AttributeTemplateData> attributeTemplates;
        public List<EntityData> entities;
    }
}
