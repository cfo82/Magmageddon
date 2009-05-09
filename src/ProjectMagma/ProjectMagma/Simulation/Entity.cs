using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;

using ProjectMagma.Simulation.Attributes;

namespace ProjectMagma.Simulation
{
    public class Entity
    {
        public event UpdateHandler Update;
        public event DrawHandler Draw;

        public Entity(string name)
        {
            this.name = name;
            this.attributes = new Dictionary<string, Attribute>();
            this.properties = new Dictionary<string, Property>();
        }

        public void Destroy()
        {
            foreach (Property property in properties.Values)
            {
                property.OnDetached(this);
            }
            properties.Clear();

            // currently no recycling necessary for attributes
            attributes.Clear();
        }

        public void OnUpdate(SimulationTime simTime)
        {
            if (Update != null)
            {
                Update(this, simTime);
            }
        }

        public void OnDraw(SimulationTime simTime, RenderMode renderMode)
        {
            if (Draw != null)
            {
                Draw(this, simTime, renderMode);
            }
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
            return Attributes.ContainsKey(attribute);
        }

        public bool IsString(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as StringAttribute) != null;
        }

        public bool IsBool(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return Attributes[attribute] is BoolAttribute;
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

        public bool IsMatrix(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            return (Attributes[attribute] as MatrixAttribute) != null;
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

        public BoolAttribute GetBoolAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsBool(attribute));
            return Attributes[attribute] as BoolAttribute;
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

        public MatrixAttribute GetMatrixAttribute(string attribute)
        {
            Debug.Assert(HasAttribute(attribute));
            Debug.Assert(IsMatrix(attribute));
            return Attributes[attribute] as MatrixAttribute;
        }

        public void SetInt(string attribute, int value)
        {
            (Attributes[attribute] as IntAttribute).Value = value;
        }

        public void SetBool(string attribute, bool value)
        {
            (Attributes[attribute] as BoolAttribute).Value = value;
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

        public void SetMatrix(string attribute, Matrix value)
        {
            (Attributes[attribute] as MatrixAttribute).Value = value;
        }

        #endregion

        #region Property Handling

        public void AddProperty(string name, string typeName)
        {
            System.Type type = System.Type.GetType(typeName);
            ConstructorInfo constructorInfo = type.GetConstructor(new System.Type[0]);
            Property property = constructorInfo.Invoke(new object[0]) as Property;
            AddProperty(name, property);
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

        public bool HasProperty(string name)
        {
            return properties.ContainsKey(name);
        }

        public Property GetProperty(string name)
        {
            return properties[name];
        }

        #endregion

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(Name).Append(": {");
            foreach (Attribute attr in Attributes.Values)
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

        public Dictionary<string, Attribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        private string name;
        private Dictionary<string, Attribute> attributes;
        private Dictionary<string, Property> properties;
    }
}
