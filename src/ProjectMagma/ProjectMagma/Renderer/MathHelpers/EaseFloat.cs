using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.MathHelpers
{
    public class EaseFloat
    {
        public EaseFloat(float value, float speed)
        {
            this.Value = value;
            this.TargetValue = value;
            this.Speed = speed;
        }

        private static readonly float referenceMilliseconds = 20.0f; // 50 fps

        public void Update(double dtMs)
        {
            float realSpeed = Speed * (float)dtMs / referenceMilliseconds;
            Value = TargetValue * realSpeed + Value * (1 - realSpeed);
        }

        public float Value { get; set; }
        public float TargetValue { get; set; }
        public float Speed { get; set; }
    }
}