using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ProjectMagma.Shared.Math.Primitives.Serialization
{
    public class Cylinder3Reader : ContentTypeReader<Cylinder3>
    {
        protected override Cylinder3 Read(ContentReader input, Cylinder3 existingInstance)
        {
            if (existingInstance == null)
            {
                existingInstance = new Cylinder3();
            }

            existingInstance.Top = input.ReadObject<Vector3>();
            existingInstance.Bottom = input.ReadObject<Vector3>();
            existingInstance.Radius = input.ReadSingle();

            return existingInstance;
        }
    }
}
