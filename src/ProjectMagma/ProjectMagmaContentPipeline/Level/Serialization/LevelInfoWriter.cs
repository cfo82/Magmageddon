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

namespace ProjectMagma.ContentPipeline.Level.Serialization
{
    [ContentTypeWriter]
    public class LevelInfoWriter : ContentTypeWriter<LevelInfo>
    {
        protected override void Write(ContentWriter output, LevelInfo levelInfo)
        {
            output.Write(levelInfo.Name);
            output.Write(levelInfo.Description);
            output.Write(levelInfo.FileName);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(LevelInfoReader).AssemblyQualifiedName;
        }
    }
}
