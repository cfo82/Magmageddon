using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ProjectMagma.MathHelpers
{
    public class SineFloat
    {
        public SineFloat(float min, float max, float frequency)
        {
            this.min = min;
            this.max = max;
            this.frequency = frequency;
        }

        public void Start(double currentTime)
        {
            startTime = currentTime;
        }

        public void StopImmediately()
        {
            startTime = 0;
            value = 0;
            stopAfterCurrentPhase = false;
        }

        public void StopAfterCurrentPhase()
        {
            if (!stopAfterCurrentPhase)
            {
                stopAfterCurrentPhase = true;
                waitOneCycle = (value - lastValue) > 0;
                valueOnStopCall = value;
            }
        }

        public void Update(double currentTime)
        {
            if(startTime != 0.0)
            {
                double time = (currentTime - startTime) * 0.001;
                lastValue = value;
                float normalized_value = (float)Math.Sin(time * frequency) / 2.0f + 0.5f;
                value = normalized_value * (max-min) + min;
                waitOneCycle = waitOneCycle && (value - lastValue) > 0;
                if(stopAfterCurrentPhase && !waitOneCycle &&
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
            get { return (startTime != 0.0);  }
        }

        private float valueOnStopCall;
        private bool waitOneCycle;
        private bool stopAfterCurrentPhase;
        private double startTime;
        private float lastValue;
        private float value;
        private float min, max;
        private float frequency;
    }
}