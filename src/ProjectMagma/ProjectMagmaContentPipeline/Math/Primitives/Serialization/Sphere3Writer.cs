using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ProjectMagma.Shared.Math.Primitives;
using ProjectMagma.Shared.Math.Primitives.Serialization;

namespace ProjectMagma.ContentPipeline.Math.Volume.Serialization
{
    [ContentTypeWriter]
    public class Sphere3Writer : ContentTypeWriter<Sphere3>
    {
        protected override void Write(ContentWriter output, Sphere3 value)
        {
            output.WriteObject<Vector3>(value.Center);
            output.Write(value.Radius);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Sphere3Reader).AssemblyQualifiedName;
        }
    }
}
