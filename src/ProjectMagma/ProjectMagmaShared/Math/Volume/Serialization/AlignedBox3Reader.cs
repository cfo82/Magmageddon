using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Volume.Serialization
{
    public class AlignedBox3Reader : ContentTypeReader<AlignedBox3>
    {
        protected override AlignedBox3 Read(ContentReader input, AlignedBox3 existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new AlignedBox3();
            }

            existingInstance.Min = input.ReadObject<Vector3>();
            existingInstance.Max = input.ReadObject<Vector3>();

            return existingInstance;
        }
    }
}
