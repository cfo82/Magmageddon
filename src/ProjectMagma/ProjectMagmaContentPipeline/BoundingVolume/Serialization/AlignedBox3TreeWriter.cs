using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.BoundingVolume;
using ProjectMagma.Shared.BoundingVolume.Serialization;

namespace ProjectMagma.ContentPipeline.BoundingVolume.Serialization
{
    [ContentTypeWriter]
    public class AlignedBox3TreeWriter : ContentTypeWriter<AlignedBox3Tree>
    {
        protected override void Write(ContentWriter output, AlignedBox3Tree value)
        {
            output.WriteObject<Vector3[]>(value.Positions);
            output.WriteSharedResource<UInt16[]>(value.Indices);
            output.WriteObject<AlignedBox3TreeNode>(value.Root);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AlignedBox3TreeReader).AssemblyQualifiedName;
        }
    }
}
