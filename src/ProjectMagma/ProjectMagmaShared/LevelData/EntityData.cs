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
}
