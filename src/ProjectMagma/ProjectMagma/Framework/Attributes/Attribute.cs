using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Framework
{
    public abstract class Attribute
    {
        public Attribute(string name)
        {
            this.name = name;
        }

        public abstract void Initialize(string value);

        public String Name
        {
            get
            {
                return this.name;
            }
        }

        public abstract String StringValue { get; }

        public static AttributeTypes GetTypeFromString(string type)
        {
            if (type == "string")
            {
                return AttributeTypes.String;
            }
            else if (type == "int")
            {
                return AttributeTypes.Int;
            }
            else if (type == "float")
            {
                return AttributeTypes.Float;
            }
            else if (type == "float2")
            {
                return AttributeTypes.Vector2;
            }
            else if (type == "float3")
            {
                return AttributeTypes.Vector3;
            }
            else if (type == "quaternion")
            {
                return AttributeTypes.Quaternion;
            }
            else
            {
                throw new Exception("invalid type-string ('" + type + "'). cannot convert it!");
            }
        }

        public static string GetTypeString(AttributeTypes type)
        {
            switch (type)
            {
                case AttributeTypes.String: return "string";
                case AttributeTypes.Int: return "int";
                case AttributeTypes.Float: return "float";
                case AttributeTypes.Vector2: return "float2";
                case AttributeTypes.Vector3: return "float3";
                case AttributeTypes.Quaternion: return "quaternion";
                default: throw new Exception("AttributeType '" + type + "' does not exist!");
            }
        }

        public override string ToString()
        {
            return Name + ": " + StringValue;
        }

        private string name;
    }
}
