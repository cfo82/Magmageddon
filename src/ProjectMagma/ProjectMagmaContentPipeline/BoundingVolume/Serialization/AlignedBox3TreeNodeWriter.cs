using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.BoundingVolume;
using ProjectMagma.Shared.BoundingVolume.Serialization;

namespace ProjectMagma.ContentPipeline.BoundingVolume.Serialization
{
    [ContentTypeWriter]
    public class AlignedBox3TreeNodeWriter : ContentTypeWriter<AlignedBox3TreeNode>
    {
        protected override void Write(ContentWriter output, AlignedBox3TreeNode value)
        {
            output.Write(value.NumTriangles);
            output.Write(value.BaseIndex);
            output.WriteSharedResource<UInt16[]>(value.Indices);
            output.WriteObject<BoundingBox>(value.BoundingBox);
            output.WriteObject<AlignedBox3TreeNode>(value.Left);
            output.WriteObject<AlignedBox3TreeNode>(value.Right);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AlignedBox3TreeNodeReader).AssemblyQualifiedName;
        }
    }
}
