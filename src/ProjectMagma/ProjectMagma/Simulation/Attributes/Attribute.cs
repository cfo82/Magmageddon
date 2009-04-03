
namespace ProjectMagma.Simulation.Attributes
{
    public abstract class Attribute
    {
        public Attribute(string name)
        {
            this.name = name;
        }

        public abstract void Initialize(string value);

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public abstract string StringValue { get; }

        public static AttributeTypes GetTypeFromString(string type)
        {
            for (int i = 0; i < stringTypeMappings.Length; ++i)
            {
                if (((string)stringTypeMappings[i, 0]) == type)
                {
                    return (AttributeTypes)stringTypeMappings[i, 1];
                }
            }

            throw new System.Exception("invalid type-string ('" + type + "'). cannot convert it!");
        }

        public static string GetTypeString(AttributeTypes type)
        {
            for (int i = 0; i < stringTypeMappings.Length; ++i)
            {
                if (((AttributeTypes)stringTypeMappings[i, 1]) == type)
                {
                    return (string)stringTypeMappings[i, 0];
                }
            }

            throw new System.Exception(string.Format("{0} is not a valid type!", type));
        }

        public override string ToString()
        {
            return Name + ": " + StringValue;
        }

        private string name;
        private static object[,] stringTypeMappings = {
            { "string",     AttributeTypes.String },
            { "int",        AttributeTypes.Int },
            { "bool",       AttributeTypes.Bool },
            { "float",      AttributeTypes.Float },
            { "float2",     AttributeTypes.Vector2 },
            { "float3",     AttributeTypes.Vector3 },
            { "quaternion", AttributeTypes.Quaternion }
        };
    }
}
