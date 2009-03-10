using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace ProjectMagma.Shared.Serialization.LevelData
{
    class AttributeTemplateDataReader : ContentTypeReader<AttributeTemplateData>
    {
        protected override AttributeTemplateData Read(ContentReader input, AttributeTemplateData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new AttributeTemplateData();
            }

            existingInstance.name = input.ReadString();
            existingInstance.type = input.ReadString();
            
            return existingInstance;
        }
    }

    class AttributeDataReader : ContentTypeReader<AttributeData>
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

    class EntityDataReader : ContentTypeReader<EntityData>
    {
        protected override EntityData Read(ContentReader input, EntityData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new EntityData();
            }

            existingInstance.name = input.ReadString();
            existingInstance.attributes = input.ReadRawObject<List<AttributeData>>();

            return existingInstance;
        }
    }

    class LevelDataReader : ContentTypeReader<LevelData>
    {
        protected override LevelData Read(ContentReader input, LevelData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new LevelData();
            }

            existingInstance.attributeTemplates = input.ReadRawObject<List<AttributeTemplateData>>();
            existingInstance.entities = input.ReadRawObject<List<EntityData>>();

            return existingInstance;
        }
    }
}
