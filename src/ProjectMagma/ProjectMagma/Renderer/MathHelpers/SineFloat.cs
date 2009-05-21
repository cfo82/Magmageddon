using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.MathHelpers
{
    public class SineFloat
    {
        public SineFloat(float min, float max, float frequency
            //float velocity
            )
        {
            this.min = min;
            this.max = max;
            this.frequency = frequency;
            //this.velocity = velocity;
        }

        public void Start(double currentTime)
        {
            start_time = currentTime;
        }

        public void StopImmediately()
        {
            start_time = 0;
            value = 0;
            stop_after_current_phase = false;
        }

        public void StopAfterCurrentPhase()
        {
            if (!stop_after_current_phase)
            {
                stop_after_current_phase = true;
                waitOneCycle = (value - lastValue) > 0;
                value_on_stop_call = value;
            }
        }

        public void Update(double currentTime)
        {
            if(start_time != 0.0)
            {
                double time = (currentTime - start_time) * 0.001;
                lastValue = value;
                float normalized_value = (float)Math.Sin(time * frequency) / 2.0f + 0.5f;
                value = normalized_value * (max-min) + min;
                waitOneCycle = waitOneCycle && (value - lastValue) > 0;
                if(stop_after_current_phase && !waitOneCycle &&
                    (value - lastValue) > 0)
                {
                    StopImmediately();
                }
            }
        }

        public float Value {
            get { return value; }
        }

        public bool Running {
            get { return (start_time != 0.0);  }
        }

        private float value_on_stop_call;
        private bool waitOneCycle;
        private bool stop_after_current_phase;
        private double start_time;
        private float lastValue;
        private float value;
        private float min, max;
        private float frequency;
       // private float velocity;
    }
}