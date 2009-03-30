using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Math.Volume;
using ProjectMagma.Shared.Math.Volume.Serialization;

namespace ProjectMagma.ContentPipeline.Math.Volume.Serialization
{
    [ContentTypeWriter]
    public class VolumeWriter : ContentTypeWriter<ProjectMagma.Shared.Math.Volume.Volume>
    {
        protected override void Write(ContentWriter output, ProjectMagma.Shared.Math.Volume.Volume value)
        {
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(VolumeReader).AssemblyQualifiedName;
        }
    }
}
