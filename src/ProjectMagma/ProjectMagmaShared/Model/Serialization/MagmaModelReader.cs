using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.Shared.Model.Serialization
{
    public class MagmaModelReader : ContentTypeReader<MagmaModel>
    {
        protected override MagmaModel Read(ContentReader input, MagmaModel existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new MagmaModel();
            }

            existingInstance.XnaModel = input.ReadObject<Microsoft.Xna.Framework.Graphics.Model>();
            existingInstance.VolumeCollection = input.ReadObject<VolumeCollection[]>();

            return existingInstance;
        }
    }
}
