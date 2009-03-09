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

namespace ProjectMagma.Shared.Serialization.LevelData
{
    [ContentTypeWriter]
    class LevelDataWriter : ContentTypeWriter<LevelData>
    {
        private void WriteAttributeTemplateData(ContentWriter output, AttributeTemplateData value)
        {
            output.Write(value.name);
            output.Write(value.type);
        }

        private void WriteAttributeData(ContentWriter output, AttributeData value)
        {
            output.Write(value.template);
            output.Write((Int32)value.values.Length);
            foreach (float f in value.values)
            {
                output.Write((Single)f);
            }
        }

        private void WriteEntityData(ContentWriter output, EntityData value)
        {
            output.Write(value.name);
            output.Write((Int32)value.attributes.Count);
            foreach (AttributeData attr in value.attributes)
            {
                WriteAttributeData(output, attr);
            }
        }

        protected override void Write(ContentWriter output, LevelData value)
        {
            output.Write((Int32)value.attributeTemplates.Count);
            foreach (AttributeTemplateData template in value.attributeTemplates)
            {
                WriteAttributeTemplateData(output, template);
            }
            output.Write((Int32)value.entities.Count);
            foreach (EntityData entity in value.entities)
            {
                WriteEntityData(output, entity);
            }
        }

        /*public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "x743.Import.Level, " + X.AssemblyName + ", Version=1.0.0.0, Culture=neutral";
        }*/

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "ProjectMagma.Shared.Serialization.LevelData.LevelDataReader, ProjectMagmaShared, Version=1.0.0.0, Culture=neutral";
        }
    }
}
