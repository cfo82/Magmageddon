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
    [ContentProcessor(DisplayName = "Magma - Explosion Processor")]
    public class ExplosionProcessor : MoveProcessor
    {
    }
}
