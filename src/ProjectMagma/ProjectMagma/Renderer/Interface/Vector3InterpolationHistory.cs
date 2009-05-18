using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace ProjectMagma.Renderer.Interface
{
    public class Vector3InterpolationHistory : InterpolationHistory<Vector3>
    {
        public Vector3InterpolationHistory(double startTimestamp, Vector3 startValue)
        :   base(startTimestamp, startValue)
        {
        }

        public override void Interpolate(ref Vector3 valFrom, ref Vector3 valTo, float amount, out Vector3 returnValue)
        {
            Debug.Assert(amount >= 0 && amount <= 1);
            Vector3.Lerp(ref valFrom, ref valTo, amount, out returnValue);
        }
    }
}
