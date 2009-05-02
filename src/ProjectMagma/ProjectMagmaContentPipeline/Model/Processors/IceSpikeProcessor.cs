using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace ProjectMagma.ContentPipeline.ModelProcessors
{
    [ContentProcessor(DisplayName = "Magma - Ice Spike Processor")]
    public class IceSpikeProcessor : MagmaModelProcessor<ModelProcessor>
    {
        protected override Vector3 CalculateDiff(ref Vector3 origDiff, ref ProjectMagma.Shared.Math.Primitives.AlignedBox3 bb)
        {
            return new Vector3(-bb.Max.X, 0, 0);
        }
    }
}
