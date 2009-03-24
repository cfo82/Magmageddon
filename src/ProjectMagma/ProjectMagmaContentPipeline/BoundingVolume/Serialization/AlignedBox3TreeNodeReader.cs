using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace ProjectMagma.Shared.BoundingVolume.Serialization
{
    class AlignedBox3TreeNodeReader : ContentTypeReader<AlignedBox3TreeNode>
    {
        protected override AlignedBox3TreeNode Read(ContentReader input, AlignedBox3TreeNode existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new AlignedBox3TreeNode();
            }

            existingInstance.NumTriangles = input.ReadInt32();
            existingInstance.BaseIndex = input.ReadInt32();
            input.ReadSharedResource<UInt16[]>(delegate(UInt16[] value) { existingInstance.Indices = value; });
            existingInstance.BoundingBox = input.ReadObject<BoundingBox>();
            existingInstance.Left = input.ReadObject<AlignedBox3TreeNode>();
            existingInstance.Right = input.ReadObject<AlignedBox3TreeNode>();

            return existingInstance;
        }
    }
}
