using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.ContentPipeline.Model
{
    public class MagmaModelContent
    {
        public Microsoft.Xna.Framework.Content.Pipeline.Processors.ModelContent XnaModel;
        public VolumeCollection[] VolumeCollection;
    }
}
