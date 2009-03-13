﻿using System;
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
    [ContentProcessor(DisplayName = "Magma - PowerUp")]
    class PowerUpProcessor : MoveProcessor
    {
        protected override float CalculateHeightDiff(ref BoundingBox bb)
        {
            return 0.0f - bb.Min.Y;
        }
    }
}
