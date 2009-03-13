using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Shared.Serialization.LevelData
{
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

    public class PropertyData
    {
        public PropertyData()
        {
            name = "";
            type = "";
        }

        public string name;
        public string type;
    }

    public class EntityData
    {
        public EntityData()
        {
            this.name = "";
            attributes = new List<AttributeData>();
            properties = new List<PropertyData>();
        }

        //public EntityData(string name)
        //{
        //    this.name = name;
        //    attributes = new List<AttributeData>();
        //    properties = new List<PropertyData>();
        //}

        public string name;
        public List<AttributeData> attributes;
        public List<PropertyData> properties;
    }

    public class LevelData
    {
        public LevelData()
        {
            entities = new List<EntityData>();
        }

        public List<EntityData> entities;
    }
}
