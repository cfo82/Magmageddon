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

namespace ProjectMagma.Shared.BoundingVolume.Serialization
{
    [ContentTypeWriter]
    class AlignedBox3TreeNodeWriter : ContentTypeWriter<AlignedBox3TreeNode>
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
