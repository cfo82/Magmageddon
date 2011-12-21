using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Shared.Math.Primitives.Serialization;

namespace ProjectMagma.ContentPipeline.Math.Primitives.Serialization
{
    [ContentTypeWriter]
    public class VolumeWriter : ContentTypeWriter<ProjectMagma.Shared.Math.Primitives.Volume>
    {
        protected override void Write(ContentWriter output, ProjectMagma.Shared.Math.Primitives.Volume value)
        {
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(VolumeReader).AssemblyQualifiedName;
        }
    }
}
