using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Shared.Math.Primitives.Serialization;

namespace ProjectMagma.ContentPipeline.Math.Volume.Serialization
{
    [ContentTypeWriter]
    public class VolumeCollectionWriter : ContentTypeWriter<VolumeCollection>
    {
        protected override void Write(ContentWriter output, VolumeCollection value)
        {
            output.WriteObject<Dictionary<VolumeType, ProjectMagma.Shared.Math.Primitives.Volume>>(value.Volumes);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(VolumeCollectionReader).AssemblyQualifiedName;
        }
    }
}
