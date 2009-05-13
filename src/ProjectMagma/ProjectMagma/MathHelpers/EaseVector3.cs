using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.MathHelpers
{
    public class EaseVector3
    {
        public EaseVector3(Vector3 value, float speed)
        {
            this.Value = value;
            this.TargetValue = value;
            this.Speed = speed;
        }

        private static readonly float referenceMilliseconds = 20.0f; // 50 fps

        public void Update(GameTime gameTime)
        {

            float realSpeed = Speed * gameTime.ElapsedGameTime.Milliseconds/referenceMilliseconds;
            Value = TargetValue * realSpeed + Value * (1 - realSpeed);
        }

        public Vector3 Value { get; set; }
        public Vector3 TargetValue { get; set; }
        public float Speed { get; set; }
    }
}