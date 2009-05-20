using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Simulation
{
    public class SimulationTime
    {
        private int frame = 0;

        private readonly double start;
        private double at = 0;
        private double last = 0;

        private double dt = 0;
        private double dtMs = 0;
        
        private double adjustmentMs = 100;

        public SimulationTime(double at)
        {
            this.start = this.last = this.at = at + adjustmentMs;
        }

        /// <summary>
        /// the how manieth frame this is since simulation start
        /// </summary>
        public int Frame
        {
            get { return frame; }
        }

        /// <summary>
        /// time in total milliseconds passed since simulation start
        /// </summary>
        public float Elapsed
        {
            get { return (float)(at - start); }
        }
        /// <summary>
        /// current time in total milliseconds passed since game start
        /// </summary>
        public float At
        {
            get { return (float)at; }
        }

        /// <summary>
        /// last time in total milliseconds passed since game start
        /// </summary>
        public float Last
        {
            get { return (float)last; }
        }

        /// <summary>
        /// difference between last and current update in fraction of a second
        /// </summary>
        public float Dt
        {
            get { return (float)dt; }
        }

        /// <summary>
        /// difference between last and current update in milliseconds
        /// </summary>
        public float DtMs
        {
            get { return (float)dtMs; }
        }

        internal void Update()
        {
            // reset last
            last = at;
            double millis = Game.Instance.GlobalClock.PausableMilliseconds;
            dtMs = (millis + adjustmentMs) - last;
            dt = dtMs / 1000d;
            at += dtMs;
            // increase frame counter
            ++frame;
        }

        public static double GetDt(double from, double to)
        {
            return (to - from) / 1000d;
        }
    }
}
