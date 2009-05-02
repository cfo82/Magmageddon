using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Primitives.Serialization
{
    public class AlignedBox3Reader : ContentTypeReader<AlignedBox3>
    {
        protected override AlignedBox3 Read(ContentReader input, AlignedBox3 existingInstance)
        {
            existingInstance.Min = input.ReadObject<Vector3>();
            existingInstance.Max = input.ReadObject<Vector3>();
            return existingInstance;
        }
    }
}
