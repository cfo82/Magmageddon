using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Shared.Math.Primitives.Serialization;

namespace ProjectMagma.ContentPipeline.Math.Volume.Serialization
{
    [ContentTypeWriter]
    public class Cylinder3Writer : ContentTypeWriter<Cylinder3>
    {
        protected override void Write(ContentWriter output, Cylinder3 value)
        {
            output.WriteObject<Vector3>(value.Top);
            output.WriteObject<Vector3>(value.Bottom);
            output.Write(value.Radius);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Cylinder3Reader).AssemblyQualifiedName;
        }
    }
}
