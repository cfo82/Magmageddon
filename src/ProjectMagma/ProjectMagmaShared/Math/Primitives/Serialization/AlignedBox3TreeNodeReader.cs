using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Primitives.Serialization
{
    public class AlignedBox3TreeNodeReader : ContentTypeReader<AlignedBox3TreeNode>
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
            existingInstance.BoundingBox = input.ReadObject<AlignedBox3>();
            existingInstance.Left = input.ReadObject<AlignedBox3TreeNode>();
            existingInstance.Right = input.ReadObject<AlignedBox3TreeNode>();

            return existingInstance;
        }
    }
}
