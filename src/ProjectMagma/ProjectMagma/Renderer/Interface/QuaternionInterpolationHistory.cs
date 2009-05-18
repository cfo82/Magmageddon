using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.Interface
{
    public class QuaternionInterpolationHistory : InterpolationHistory<Quaternion>
    {
        public QuaternionInterpolationHistory(double startTimestamp, Quaternion startValue)
        :   base(startTimestamp, startValue)
        {
        }

        public override void Interpolate(ref Quaternion valFrom, ref Quaternion valTo, float amount, out Quaternion returnValue)
        {
            Debug.Assert(amount >= 0 && amount <= 1);
            Quaternion.Slerp(ref valFrom, ref valTo, amount, out returnValue);
        }
    }
}
