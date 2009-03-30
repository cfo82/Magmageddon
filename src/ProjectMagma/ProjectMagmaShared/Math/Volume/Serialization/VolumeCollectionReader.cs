using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Volume.Serialization
{
    public class VolumeCollectionReader : ContentTypeReader<VolumeCollection>
    {
        protected override VolumeCollection Read(ContentReader input, VolumeCollection existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new VolumeCollection();
            }

            existingInstance.Volumes = input.ReadObject<Dictionary<VolumeType, Volume>>();

            return existingInstance;
        }
    }
}
