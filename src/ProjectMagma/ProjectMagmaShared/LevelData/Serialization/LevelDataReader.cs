using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.LevelData.Serialization
{

    public class AttributeDataReader : ContentTypeReader<AttributeData>
    {
        protected override AttributeData Read(ContentReader input, AttributeData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new AttributeData();
            }

            existingInstance.name = input.ReadString();
            existingInstance.template = input.ReadString();
            existingInstance.value = input.ReadString();

            return existingInstance;
        }
    }

    public class PropertyDataReader : ContentTypeReader<PropertyData>
    {
        protected override PropertyData Read(ContentReader input, PropertyData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new PropertyData();
            }

            existingInstance.name = input.ReadString();
            existingInstance.type = input.ReadString();

            return existingInstance;
        }
    }

    public class EntityDataReader : ContentTypeReader<EntityData>
    {
        protected override EntityData Read(ContentReader input, EntityData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new EntityData();
            }

            existingInstance.isAbstract = input.ReadBoolean();
            existingInstance.name = input.ReadString();
            existingInstance.parent = input.ReadString();
            existingInstance.attributes = input.ReadRawObject<List<AttributeData>>();
            existingInstance.properties = input.ReadRawObject<List<PropertyData>>();

            return existingInstance;
        }
    }

    public class LevelDataReader : ContentTypeReader<LevelData>
    {
        protected override LevelData Read(ContentReader input, LevelData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new LevelData();
            }

            existingInstance.entities = input.ReadRawObject<Dictionary<string, EntityData>>();

            return existingInstance;
        }
    }
}
