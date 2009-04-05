using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.LevelData.Serialization
{
    public class RobotInfoReader : ContentTypeReader<RobotInfo>
    {
        protected override RobotInfo Read(ContentReader input, RobotInfo existingInstance)
        {
            return new RobotInfo(input.ReadString(), input.ReadString(), input.ReadString());
        }
    }

}
