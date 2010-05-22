using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Simulation
{
    public class OrientationHelper
    {
        public static void CalculateWorldTransform(
            Entity entity,
            out Matrix world
        )
        {
            Vector3 translation, scale;
            Quaternion rotation;
            GetScale(entity, out scale);
            GetRotation(entity, out rotation);
            GetTranslation(entity, out translation);
            CalculateWorldTransform(ref translation, ref rotation, ref scale, out world);
        }

        public static void CalculateWorldTransform(
            ref Vector3 translation,
            ref Quaternion rotation,
            ref Vector3 scale,
            out Matrix world
        )
        {
            Matrix scaleMatrix, rotationMatrix, translationMatrix, tempMatrix;
            Matrix.CreateScale(ref scale, out scaleMatrix);
            Matrix.CreateFromQuaternion(ref rotation, out rotationMatrix);
            Matrix.CreateTranslation(ref translation, out translationMatrix);
            Matrix.Multiply(ref scaleMatrix, ref rotationMatrix, out tempMatrix);
            Matrix.Multiply(ref tempMatrix, ref translationMatrix, out world);
        }

        public static void GetTranslation(
            Entity entity,
            out Vector3 translation
        )
        {
            translation = entity.GetVector3(CommonNames.Position);
        }

        public static void GetScale(
            Entity entity,
            out Vector3 scale
        )
        {
            if (entity.HasVector3(CommonNames.Scale))
            {
                scale = entity.GetVector3(CommonNames.Scale);
            }
            else
            {
                scale = Vector3.One;
            }
        }

        public static void GetRotation(
            Entity entity,
            out Quaternion rotation
        )
        {
            if (entity.HasQuaternion(CommonNames.Rotation))
            {
                rotation = entity.GetQuaternion(CommonNames.Rotation);
            }
            else
            {
                rotation = Quaternion.Identity;
            }
        }
    }
}
