using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Primitives.Serialization
{
    public class Sphere3Reader : ContentTypeReader<Sphere3>
    {
        protected override Sphere3 Read(ContentReader input, Sphere3 existingInstance)
        {
            existingInstance.Center = input.ReadObject<Vector3>();
            existingInstance.Radius = input.ReadSingle();
            return existingInstance;
        }
    }
}
