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
    class AlignedBox3TreeReader : ContentTypeReader<AlignedBox3Tree>
    {
        protected override AlignedBox3Tree Read(ContentReader input, AlignedBox3Tree existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new AlignedBox3Tree();
            }

            existingInstance.Positions = input.ReadObject<Vector3[]>();
            input.ReadSharedResource<UInt16[]>(delegate(UInt16[] value) { existingInstance.Indices = value; });
            existingInstance.Root = input.ReadObject<AlignedBox3TreeNode>();

            return existingInstance;
        }
    }
}
