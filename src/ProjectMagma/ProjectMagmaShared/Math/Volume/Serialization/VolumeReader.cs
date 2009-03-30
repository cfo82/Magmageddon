using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Volume.Serialization
{
    public class VolumeReader : ContentTypeReader<Volume>
    {
        protected override Volume Read(ContentReader input, Volume existingInstance)
        {
            Debug.Assert(existingInstance != null);
            return existingInstance;
        }
    }
}
