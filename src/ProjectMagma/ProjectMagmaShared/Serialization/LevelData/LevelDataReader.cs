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
    class LevelDataReader : ContentTypeReader<LevelData>
    {
        private AttributeTemplateData ReadAttributeTemplateData(ContentReader input)
        {
            AttributeTemplateData templateData = new AttributeTemplateData();
            templateData.name = input.ReadString();
            templateData.type = input.ReadString();
            return templateData;
        }

        private AttributeData ReadAttributeData(ContentReader input)
        {
            AttributeData attributeData = new AttributeData();
            attributeData.template = input.ReadString();
            attributeData.values = new float[input.ReadInt32()];
            for (int i = 0; i < attributeData.values.Length; ++i)
            {
                attributeData.values[i] = input.ReadSingle();
            }
            return attributeData;
        }

        private EntityData ReadEntityData(ContentReader input)
        {
            EntityData entityData = new EntityData();
            entityData.name = input.ReadString();
            int attributeCount = input.ReadInt32();
            for (int i = 0; i < attributeCount; ++i)
            {
                entityData.attributes.Add(ReadAttributeData(input));
            }
            return entityData;
        }

        protected override LevelData Read(ContentReader input, LevelData existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new LevelData();
            }

            int attributeTemplateCount = input.ReadInt32();
            for (int i = 0; i < attributeTemplateCount; ++i)
            {
                existingInstance.attributeTemplates.Add(ReadAttributeTemplateData(input));
            }

            int entityCount = input.ReadInt32();
            for (int i = 0; i < entityCount; ++i)
            {
                existingInstance.entities.Add(ReadEntityData(input));
            }

            return existingInstance;
        }
    }
}
