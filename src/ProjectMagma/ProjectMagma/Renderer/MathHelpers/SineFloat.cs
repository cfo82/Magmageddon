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
        }

        public void StopAfterCurrentPhase()
        {
            stop_after_current_phase = true;
            value_on_stop_call = value;
        }

        public void Update(double currentTime)
        {
            if(start_time != 0.0)
            {
                double time = (currentTime - start_time) * 0.001;
                float normalized_value = (float)Math.Sin(time * frequency) / 2.0f + 0.5f;
                value = normalized_value * (max-min) + min;
                if(stop_after_current_phase &&
                    Math.Sign(value_on_stop_call) != Math.Sign(value))
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
        private bool stop_after_current_phase;
        private double start_time;
        private float value;
        private float min, max;
        private float frequency;
       // private float velocity;
    }
}