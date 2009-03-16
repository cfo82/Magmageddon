using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Shared.Serialization.LevelData;

namespace ProjectMagma.Framework
{
    public class Entity
    {
        public Entity(EntityManager entityManager, string name)
        {
            this.entityManager = entityManager;
            this.name = name;
            this.attributes = new Dictionary<string, Attribute>();
            this.properties = new Dictionary<string, Property>();
        }

        public event UpdateHandler Update;
        public event DrawHandler Draw;

        public void OnUpdate(GameTime gameTime)
        {
            if (Update != null)
            {
                Update(this, gameTime);
            }
        }

        public void OnDraw(GameTime gameTime, RenderMode renderMode)
        {
            if (Draw != null)
            {
                Draw(this, gameTime, renderMode);
            }
        }

        #region Attribute Handling 

        public void AddAttribute(AttributeData attributeData)
        {
            AddAttribute(attributeData.name, attributeData.template, attributeData.value);
        }

        public void AddAttribute(string name, AttributeTypes type)
        {
            AddAttribute(name, type, "");
        }

        public void AddAttribute(string name, string type, string value)
        {
            AddAttribute(name, Attribute.GetTypeFromString(type), value);
        }

        public void AddAttribute(string name, AttributeTypes type, string value)
        {
            Attribute attribute = null;
            switch (type)
            {
                case AttributeTypes.String: attribute = new StringAttribute(name); break;
                case AttributeTypes.Int: attribute = new IntAttribute(name); break;
                case AttributeTypes.Float: attribute = new FloatAttribute(name); break;
                case AttributeTypes.Vector2: attribute = new Vector2Attribute(name); break;
                case AttributeTypes.Vector3: attribute = new Vector3Attribute(name); break;
                case AttributeTypes.Quaternion: attribute = new QuaternionAttribute(name); break;
                default: throw new Exception("AttributeType '" + type + "' does not exist!");
            }
            attribute.Initialize(value);
            this.attributes.Add(attribute.Name, attribute);
        }

        public void AddStringAttribute(string name, string value)
        {
            StringAttribute attribute = new StringAttribute(name);
            attribute.Value = value;
            this.attributes.Add(attribute.Name, attribute);
        }

        public void AddIntAttribute(string name, int value)
        {
            IntAttribute attribute = new IntAttribute(name);
            attribute.Value = value;
            this.attributes.Add(attribute.Name, attribute);
        }

        public void AddFloatAttribute(string name, float value)
        {
            FloatAttribute attribute = new FloatAttribute(name);
            attribute.Value = value;
            this.attributes.Add(attribute.Name, attribute);
        }

        public void AddVector2Attribute(string name, Vector2 value)
        {
            Vector2Attribute attribute = new Vector2Attribute(name);
            attribute.Value = value;
            this.attributes.Add(attribute.Name, attribute);
        }

        public void AddVector3Attribute(string name, Vector3 value)
        {
            Vector3Attribute attribute = new Vector3Attribute(name);
            attribute.Value = value;
            this.attributes.Add(attribute.Name, attribute);
        }

        public void AddQuaternionAttribute(string name, Quaternion value)
        {
            QuaternionAttribute attribute = new QuaternionAttribute(name);
            attribute.Value = value;
            this.attributes.Add(attribute.Name, attribute);
        }

        public bool HasAttribute(string attribute)
        {
            return Attributes.ContainsKey(attribute);
        }

        public bool IsString(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as StringAttribute) != null;
        }

        public bool IsInt(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as IntAttribute) != null;
        }

        public bool IsFloat(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as FloatAttribute) != null;
        }

        public bool IsVector2(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as Vector2Attribute) != null;
        }

        public bool IsVector3(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as Vector3Attribute) != null;
        }

        public bool IsQuaternion(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as QuaternionAttribute) != null;
        }

        public bool HasString(string attribute)
        {
            return HasAttribute(attribute) && IsString(attribute);
        }

        public bool HasInt(string attribute)
        {
            return HasAttribute(attribute) && IsInt(attribute);
        }

        public bool HasFloat(string attribute)
        {
            return HasAttribute(attribute) && IsFloat(attribute);
        }

        public bool HasVector2(string attribute)
        {
            return HasAttribute(attribute) && IsVector2(attribute);
        }

        public bool HasVector3(string attribute)
        {
            return HasAttribute(attribute) && IsVector3(attribute);
        }

        public bool HasQuaternion(string attribute)
        {
            return HasAttribute(attribute) && IsQuaternion(attribute);
        }

        public string GetString(string attribute)
        {
            return GetStringAttribute(attribute).Value;
        }

        public int GetInt(string attribute)
        {
            return GetIntAttribute(attribute).Value;
        }

        public float GetFloat(string attribute)
        {
            return GetFloatAttribute(attribute).Value;
        }

        public Vector2 GetVector2(string attribute)
        {
            return GetVector2Attribute(attribute).Value;
        }

        public Vector3 GetVector3(string attribute)
        {
            return GetVector3Attribute(attribute).Value;
        }

        public Quaternion GetQuaternion(string attribute)
        {
            return GetQuaternionAttribute(attribute).Value;
        }

        public Attribute GetAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return Attributes[attribute];
        }

        public StringAttribute GetStringAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsString(attribute));
            return Attributes[attribute] as StringAttribute;
        }

        public IntAttribute GetIntAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsInt(attribute));
            return Attributes[attribute] as IntAttribute;
        }

        public FloatAttribute GetFloatAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsFloat(attribute));
            return Attributes[attribute] as FloatAttribute;
        }

        public Vector2Attribute GetVector2Attribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsVector2(attribute));
            return Attributes[attribute] as Vector2Attribute;
        }

        public Vector3Attribute GetVector3Attribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsVector3(attribute));
            return Attributes[attribute] as Vector3Attribute;
        }

        public QuaternionAttribute GetQuaternionAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsQuaternion(attribute));
            return Attributes[attribute] as QuaternionAttribute;
        }

        public void SetInt(string attribute, int value)
        {
            (Attributes[attribute] as IntAttribute).Value = value;
        }

        public void SetString(string attribute, string value)
        {
            (Attributes[attribute] as StringAttribute).Value = value;
        }

        public void SetFloat(string attribute, float value)
        {
            (Attributes[attribute] as FloatAttribute).Value = value;
        }

        public void SetVector2(string attribute, Vector2 value)
        {
            (Attributes[attribute] as Vector2Attribute).Value = value;
        }

        public void SetVector3(string attribute, Vector3 value)
        {
            (Attributes[attribute] as Vector3Attribute).Value = value;
        }

        public void SetQuaternion(string attribute, Quaternion value)
        {
            (Attributes[attribute] as QuaternionAttribute).Value = value;
        }

        #endregion

        #region Property Handling

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(Name).Append(": {");
            foreach (Attribute attr in Attributes.Values)
                str.Append(attr.ToString()).Append(";\n ");
            str.Append("}");
            return str.ToString();
        }

        public void AddProperty(PropertyData propertyData)
        {
            Type type = Type.GetType(propertyData.type);
            ConstructorInfo constructorInfo = type.GetConstructor(new Type[0]);
            Property property = constructorInfo.Invoke(new object[0]) as Property;
            AddProperty(propertyData.name, property);
        }

        public void AddProperty(string name, Property property)
        {
            if (properties.ContainsKey(name))
            {
                // TODO: duplicate property exception
                properties.Add(name, null); // throws exception
            }
            else
            {
                properties.Add(name, property);
                property.OnAttached(this);
            }
        }

        public void RemoveProperty(string name)
        {
            if (properties.ContainsKey(name))
            {
                Property property = properties[name];
                property.OnDetached(this);
                properties.Remove(name);
            }
        }

        #endregion

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public Dictionary<string, Attribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        private EntityManager entityManager;
        private string name;
        private Dictionary<string, Attribute> attributes;
        private Dictionary<string, Property> properties;
    }
}
