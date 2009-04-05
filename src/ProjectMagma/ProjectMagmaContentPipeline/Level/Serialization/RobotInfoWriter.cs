using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.LevelData.Serialization;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.ContentPipeline.Robot.Serialization
{
    [ContentTypeWriter]
    public class RobotInfoWriter : ContentTypeWriter<RobotInfo>
    {
        protected override void Write(ContentWriter output, RobotInfo RobotInfo)
        {
            output.Write(RobotInfo.Name);
            output.Write(RobotInfo.Description);
            output.Write(RobotInfo.Entity);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(RobotInfoReader).AssemblyQualifiedName;
        }
    }
}
