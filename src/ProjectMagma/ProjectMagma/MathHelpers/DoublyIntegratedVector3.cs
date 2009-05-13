using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

// IMPORTANT: this class is later going to be moved into the shared project.
// however, since changing something which is included in the content processor
// (including the math library) forces recompilation of all resources, I am 
// developing it in the main project.
// apr 16, dpk
namespace ProjectMagma.Shared.Math.Integration
{
    public class DoublyIntegratedVector3
    {
        #region constructors

        public DoublyIntegratedVector3(
            Vector3 start, Vector3 d_start
        )
        {
            this.value = start;
            this.d_value = d_start;
            this.minLength = 0.0f;
            this.maxLength = 0.0f;
            this.d_minLength = 0.0f;
            this.d_maxLength = 0.0f;
        }

        public DoublyIntegratedVector3(
            Vector3 start, Vector3 d_start,
            float minLength, float maxLength
        ) :
            this(start, d_start)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
        }

        public DoublyIntegratedVector3(
            Vector3 start, Vector3 d_start,
            float minLength, float maxLength,
            float d_minLength, float d_maxLength
        ) :
            this(start, d_start, minLength, maxLength)
        {
            this.d_minLength = d_minLength;
            this.d_maxLength = d_maxLength;
        }

        #endregion

        private bool CheckBoundsEnabled()
        {
            return minLength != maxLength;
        }

        private bool CheckDBoundsEnabled()
        {
            return d_minLength != d_maxLength;
        }

        public void Integrate(
            GameTime gameTime,
            Vector3 dd_value
        )
        {
            Vector3 old_d_value = d_value;
            Vector3 old_value = value;

            // integrate using the leap frog scheme
            d_value += dd_value * gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            value += d_value * gameTime.ElapsedGameTime.Milliseconds * 0.001f;

            // if we're out of bounds, fetch the backup
            if (CheckBoundsEnabled() && (value.LengthSquared()<minLength || value.LengthSquared()>maxLength))
            {
                value = old_value;
            }

            if (CheckDBoundsEnabled() && (d_value.LengthSquared() < d_minLength || d_value.LengthSquared() > d_maxLength))
            {
                d_value = old_d_value;
            }
        }

        public void RandomlyIntegrate
        (
            GameTime gameTime,
            float amplitude,
            float bias
            // to play around with the biasing, use the following Matlab line as a start:
            // R=[]; for i=1:25000; r=rand; R=[R r^0.3]; end; hist(R,20); mean(R)
            )
        {
            Vector3 biased_normalized_random = new Vector3
            (
                (float) System.Math.Pow(random.NextDouble(), 1 - System.Math.Abs(bias)),
                (float) System.Math.Pow(random.NextDouble(), 1 - System.Math.Abs(bias)),
                (float)System.Math.Pow(random.NextDouble(), 1 - System.Math.Abs(bias))
            );

            if (bias < 0.0f)
            {
                biased_normalized_random = Vector3.One - biased_normalized_random;
            }

            // sign and scale
            Vector3 dd_value = (2 * biased_normalized_random - Vector3.One);
            dd_value *= amplitude;

            // integrates
            Integrate(gameTime, dd_value);
        }

        private Vector3 value;
        private Vector3 d_value;
        private float minLength, maxLength;
        private float d_minLength, d_maxLength;

        private static Random random = new Random();

        public Vector3 Value
        {
            get { return value; }
        }
    }
}
