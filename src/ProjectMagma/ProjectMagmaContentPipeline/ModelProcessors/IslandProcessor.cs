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
    [ContentProcessor(DisplayName = "Magma - Island Processor")]
    public class IslandProcessor : MoveProcessor
    {
        protected override Vector3 CalculateDiff(ref Vector3 origDiff, ref BoundingBox bb)
        {
            return new Vector3(0, 0.0f - bb.Max.Y, 0);
        }
    }
}
