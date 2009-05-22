using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.LevelData.Serialization;
using ProjectMagma.Shared.LevelData;

namespace ProjectMagma.ContentPipeline.Level.Serialization
{
    [ContentTypeWriter]
    public class LevelInfoWriter : ContentTypeWriter<LevelInfo>
    {
        protected override void Write(ContentWriter output, LevelInfo levelInfo)
        {
            output.Write(levelInfo.Name);
            output.Write(levelInfo.Description);
            output.Write(levelInfo.SimulationFileName);
            output.Write(levelInfo.RendererFileName);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(LevelInfoReader).AssemblyQualifiedName;
        }
    }
}
