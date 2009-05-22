using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;

using ProjectMagma.Framework.Attributes;

namespace ProjectMagma.Framework
{
    public class AbstractEntity
    {
        public AbstractEntity(string name)
        {
            this.name = name;
            this.attributes = new Dictionary<string, Attribute>();
            this.properties = new Dictionary<string, Property>();
        }

        public void Destroy()
        {
            DetachAll();
            Clear();
        }

        public void DetachAll()
        {
            foreach (Property property in properties.Values)
            {
                property.OnDetached(this);
            }
            // currently no recycling necessary for attributes
        }

        public void Clear()
        {
            properties.Clear();
            attributes.Clear();
        }

        #region Attribute Handling 

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
                case AttributeTypes.Matrix: attribute = new MatrixAttribute(name); break;
                case AttributeTypes.Bool: attribute = new BoolAttribute(name); break;
                default: throw new System.Exception("AttributeType '" + type + "' does not exist!");
            }
            attribute.Initialize(value);
            AddAttribute(attribute);
        }

        public void AddStringAttribute(string name, string value)
        {
            AddAttribute(new StringAttribute(name, value));
        }

        public void AddIntAttribute(string name, int value)
        {
            AddAttribute(new IntAttribute(name, value));
        }

        public void AddBoolAttribute(string name, bool value)
        {
            AddAttribute(new BoolAttribute(name, value));
        }

        public void AddFloatAttribute(string name, float value)
        {
            AddAttribute(new FloatAttribute(name, value));
        }

        public void AddVector2Attribute(string name, Vector2 value)
        {
            AddAttribute(new Vector2Attribute(name, value));
        }

        public void AddVector3Attribute(string name, Vector3 value)
        {
            AddAttribute(new Vector3Attribute(name, value));
        }

        public void AddQuaternionAttribute(string name, Quaternion value)
        {
            AddAttribute(new QuaternionAttribute(name, value));
        }

        public void AddMatrixAttribute(string name, Matrix value)
        {
            AddAttribute(new MatrixAttribute(name, value));
        }

        private void AddAttribute(Attribute attribute)
        {
            if (this.attributes.ContainsKey(attribute.Name))
            {
                throw new System.Exception(string.Format("attribute with name {0} already contained!", attribute.Name));
            }

            this.attributes.Add(attribute.Name, attribute);
        }

        public bool HasAttribute(string attribute)
        {
            return attributes.ContainsKey(attribute);
        }

        public bool HasAttributeWithType<AttributeType>(string attribute)
        {
            return HasAttribute(attribute) && attributes[attribute].GetType() == typeof(AttributeType);
        }

        public bool IsString(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<StringAttribute>(attribute);
        }

        public bool IsBool(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<BoolAttribute>(attribute);
        }

        public bool IsInt(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<IntAttribute>(attribute);
        }

        public bool IsFloat(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<FloatAttribute>(attribute);
        }

        public bool IsVector2(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<Vector2Attribute>(attribute);
        }

        public bool IsVector3(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<Vector3Attribute>(attribute);
        }

        public bool IsQuaternion(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<QuaternionAttribute>(attribute);
        }

        public bool IsMatrix(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return HasAttributeWithType<MatrixAttribute>(attribute);
        }

        public bool HasString(string attribute)
        {
            return HasAttribute(attribute) && IsString(attribute);
        }

        public bool HasInt(string attribute)
        {
            return HasAttribute(attribute) && IsInt(attribute);
        }

        public bool HasBool(string attribute)
        {
            return HasAttribute(attribute) && IsBool(attribute);
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

        public bool HasMatrix(string attribute)
        {
            return HasAttribute(attribute) && IsMatrix(attribute);
        }

        public string GetString(string attribute)
        {
            return GetStringAttribute(attribute).Value;
        }

        public bool GetBool(string attribute)
        {
            return GetBoolAttribute(attribute).Value;
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

        public Matrix GetMatrix(string attribute)
        {
            return GetMatrixAttribute(attribute).Value;
        }

        public AttributeType GetAttribute<AttributeType>(string name) where AttributeType : Attribute
        {
            if (!attributes.ContainsKey(name))
            {
                throw new System.ArgumentException(string.Format("attribute '{0}' not found!", name));
            }

            Attribute attribute = attributes[name];
            if (attributes.GetType() != typeof(AttributeType))
            {
                throw new System.ArgumentException(string.Format("requesting type '{0}' but attribute has type '{1}'!", typeof(AttributeType).Name, attribute.GetType()));
            }

            return attribute as AttributeType;
        }

        public StringAttribute GetStringAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsString(attribute));
            return GetAttribute<StringAttribute>(attribute);
        }

        public IntAttribute GetIntAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsInt(attribute));
            return GetAttribute<IntAttribute>(attribute);
        }

        public BoolAttribute GetBoolAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsBool(attribute));
            return GetAttribute<BoolAttribute>(attribute);
        }

        public FloatAttribute GetFloatAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsFloat(attribute));
            return GetAttribute<FloatAttribute>(attribute);
        }

        public Vector2Attribute GetVector2Attribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsVector2(attribute));
            return GetAttribute<Vector2Attribute>(attribute);
        }

        public Vector3Attribute GetVector3Attribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsVector3(attribute));
            return GetAttribute<Vector3Attribute>(attribute);
        }

        public QuaternionAttribute GetQuaternionAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsQuaternion(attribute));
            return GetAttribute<QuaternionAttribute>(attribute);
        }

        public MatrixAttribute GetMatrixAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsMatrix(attribute));
            return GetAttribute<MatrixAttribute>(attribute);
        }

        public void SetInt(string attribute, int value)
        {
            GetAttribute<IntAttribute>(attribute).Value = value;
        }

        public void SetBool(string attribute, bool value)
        {
            GetAttribute<BoolAttribute>(attribute).Value = value;
        }

        public void SetString(string attribute, string value)
        {
            GetAttribute<StringAttribute>(attribute).Value = value;
        }

        public void SetFloat(string attribute, float value)
        {
            GetAttribute<FloatAttribute>(attribute).Value = value;
        }

        public void SetVector2(string attribute, Vector2 value)
        {
            GetAttribute<Vector2Attribute>(attribute).Value = value;
        }

        public void SetVector3(string attribute, Vector3 value)
        {
            GetAttribute<Vector3Attribute>(attribute).Value = value;
        }

        public void SetQuaternion(string attribute, Quaternion value)
        {
            GetAttribute<QuaternionAttribute>(attribute).Value = value;
        }

        public void SetMatrix(string attribute, Matrix value)
        {
            GetAttribute<MatrixAttribute>(attribute).Value = value;
        }

        #endregion

        #region Property Handling

        public void AddProperty(string name, string typeName)
        {
            System.Type type = System.Type.GetType(typeName);
            ConstructorInfo constructorInfo = type.GetConstructor(AbstractEntity.zeroTypeArray);
            Property property = constructorInfo.Invoke(AbstractEntity.zeroObjectArray) as Property;
            AddProperty(name, property);
        }

        public void AddProperty(string name, Property property)
        {
            if (properties.ContainsKey(name))
            {
                throw new System.ArgumentException(string.Format("a property with the name '{0}' is already registered", name));
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

        public bool HasProperty(string name)
        {
            return properties.ContainsKey(name);
        }

        public Property GetProperty(string name)
        {
            if (!properties.ContainsKey(name))
            {
                throw new System.ArgumentException(string.Format("property '{0}' not found!", name));
            }

            return properties[name];
        }

        #endregion

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(Name).Append(": {");
            foreach (Attribute attr in attributes.Values)
                str.Append(attr.ToString()).Append(";\n ");
            str.Append("}");
            return str.ToString();
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        private string name;
        private Dictionary<string, Attribute> attributes;
        private Dictionary<string, Property> properties;
        private static readonly System.Type[] zeroTypeArray = new System.Type[0];
        private static readonly object[] zeroObjectArray = new object[0];
    }
}
