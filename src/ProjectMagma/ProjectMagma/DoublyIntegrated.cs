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
    public class DoublyIntegratedFloat
    {
        #region constructors

        public DoublyIntegratedFloat(
            float start, float d_start
        )
        {
            this.value = start;
            this.d_value = d_start;
            this.min = 0.0f;
            this.max = 0.0f;
            this.d_min = 0.0f;
            this.d_max = 0.0f;
        }

        public DoublyIntegratedFloat(
            float start, float d_start,
            float min, float max
        ) :
            this(start, d_start)
        {
            this.min = min;
            this.max = max;
        }

        public DoublyIntegratedFloat(
            float start, float d_start,
            float min, float max,
            float d_min, float d_max
        ) : 
            this(start, d_start, min, max)
        {
            this.d_min = d_min;
            this.d_max = d_max;
        }

        #endregion

        private bool CheckBoundsEnabled()
        {
            return min != max;
        }

        private bool CheckDBoundsEnabled()
        {
            return d_min != d_max;
        }

        public void Integrate(
            GameTime gameTime,
            float dd_value
        )
        {
            float old_d_value = d_value;
            float old_value = value;

            // integrate using the leap frog scheme
            d_value += dd_value * gameTime.ElapsedGameTime.Milliseconds * 0.001f;
            value += d_value * gameTime.ElapsedGameTime.Milliseconds * 0.001f;

            // if we're out of bounds, fetch the backup
            if (CheckBoundsEnabled() && (value<min || value>max))
            {
                value = old_value;
            }

            if (CheckDBoundsEnabled() && (d_value < d_min || d_value > d_max))
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
            float biased_normalized_random = (float) System.Math.Pow(random.NextDouble(), 1 - System.Math.Abs(bias));
            if (bias < 0.0f)
            {
                biased_normalized_random = 1.0f - biased_normalized_random;
            }

            // sign and scale
            float dd_value = (2 * biased_normalized_random - 1.0f);
            dd_value *= amplitude;

            // integrates
            Integrate(gameTime, dd_value);
        }

        private float value;
        private float d_value;
        private float min, max;
        private float d_min, d_max;

        private static Random random = new Random();

        public float Value
        {
            get { return value; }
        }
    }
}
