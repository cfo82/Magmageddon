using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Volume.Serialization
{
    public class AlignedBox3TreeReader : ContentTypeReader<AlignedBox3Tree>
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
