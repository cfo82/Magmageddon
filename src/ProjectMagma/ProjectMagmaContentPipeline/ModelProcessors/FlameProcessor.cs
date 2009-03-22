using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace ProjectMagmaContentPipeline.ModelProcessors
{
    [ContentProcessor(DisplayName = "Magma - Flame Processor")]
    class FlameProcessor : MoveProcessor
    {

        protected override Vector3 CalculateDiff(ref Vector3 origDiff, ref BoundingBox bb)
        {
            return -origDiff;
        }
        
    }
}
