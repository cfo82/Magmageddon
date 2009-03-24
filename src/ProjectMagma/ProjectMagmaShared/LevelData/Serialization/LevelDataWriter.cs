using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.LevelData.Serialization;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagmaContentPipeline.Level.Serialization
{
    [ContentTypeWriter]
    class AttributeDataWriter : ContentTypeWriter<AttributeData>
    {
        protected override void Write(ContentWriter output, AttributeData value)
        {
            output.Write(value.name);
            output.Write(value.template);
            output.Write(value.value);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AttributeDataReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeWriter]
    class PropertyDataWriter : ContentTypeWriter<PropertyData>
    {
        protected override void Write(ContentWriter output, PropertyData value)
        {
            output.Write(value.name);
            output.Write(value.type);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(PropertyDataReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeWriter]
    class EntityDataWriter : ContentTypeWriter<EntityData>
    {
        protected override void Write(ContentWriter output, EntityData value)
        {
            output.Write(value.isAbstract);
            output.Write(value.name);
            output.Write(value.parent);
            output.WriteRawObject<List<AttributeData>>(value.attributes);
            output.WriteRawObject<List<PropertyData>>(value.properties);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(EntityDataReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeWriter]
    class LevelDataWriter : ContentTypeWriter<LevelData>
    {
        protected override void Write(ContentWriter output, LevelData value)
        {
            output.WriteRawObject<Dictionary<string, EntityData>>(value.entities);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(LevelDataReader).AssemblyQualifiedName;
        }
    }
}
