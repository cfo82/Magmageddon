using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.LevelData.Serialization
{
    public class LevelInfoReader : ContentTypeReader<LevelInfo>
    {
        protected override LevelInfo Read(ContentReader input, LevelInfo existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new LevelInfo();
            }

            existingInstance.Name = input.ReadString();
            existingInstance.Description = input.ReadString();
            existingInstance.SimulationFileName = input.ReadString();
            existingInstance.RendererFileName = input.ReadString();

            return existingInstance;
        }
    }

}
