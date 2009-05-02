using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.ContentPipeline.ModelProcessors
{
    [ContentProcessor(DisplayName = "Magma - Dwarf Processor")]
    public class DwarfProcessor : MoveProcessor<Xclna.Xna.Animation.Content.AnimatedModelProcessor>
    {
        protected override Vector3 CalculateDiff(ref Vector3 origDiff, ref AlignedBox3 bb)
        {
            return new Vector3(0, 0.0f - bb.Min.Y, 0);
        }

        protected override string GetContainerGroupImporter()
        {
            return typeof(Xclna.Xna.Animation.Content.XModelImporter).Name;
        }
    }
}
