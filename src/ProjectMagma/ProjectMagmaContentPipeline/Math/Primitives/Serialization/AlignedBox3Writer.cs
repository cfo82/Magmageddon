using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Shared.Math.Primitives.Serialization;

namespace ProjectMagma.ContentPipeline.Math.Primitives.Serialization
{
    [ContentTypeWriter]
    public class AlignedBox3Writer : ContentTypeWriter<AlignedBox3>
    {
        protected override void Write(ContentWriter output, AlignedBox3 value)
        {
            output.WriteObject<Vector3>(value.Min);
            output.WriteObject<Vector3>(value.Max);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(AlignedBox3Reader).AssemblyQualifiedName;
        }
    }
}
