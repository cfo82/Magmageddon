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
    class AlignedBox3TreeWriter : ContentTypeWriter<AlignedBox3Tree>
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
