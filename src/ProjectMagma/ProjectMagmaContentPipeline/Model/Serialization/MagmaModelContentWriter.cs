using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Model;
using ProjectMagma.Shared.Model.Serialization;
using ProjectMagma.Shared.Math.Primitives;

namespace ProjectMagma.ContentPipeline.Model.Serialization
{
    [ContentTypeWriter]
    public class MagmaModelContentWriter : ContentTypeWriter<MagmaModelContent>
    {
        protected override void Write(ContentWriter output, MagmaModelContent value)
        {
            output.WriteObject<Microsoft.Xna.Framework.Content.Pipeline.Processors.ModelContent>(value.XnaModel);
            output.WriteObject<VolumeCollection[]>(value.VolumeCollection);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(MagmaModelReader).AssemblyQualifiedName;
        }
    }
}
