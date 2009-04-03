using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.LevelData.Serialization
{
    public class LevelInfoReader : ContentTypeReader<LevelInfo>
    {
        protected override LevelInfo Read(ContentReader input, LevelInfo existingInstance)
        {
            return new LevelInfo(input.ReadString(), input.ReadString(), input.ReadString());
        }
    }

}
