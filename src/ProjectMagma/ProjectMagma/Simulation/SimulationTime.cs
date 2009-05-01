using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Simulation
{
    public class SimulationTime
    {
        private long lastTick = DateTime.Now.Ticks;

        private int frame = 0;

        private float at = 0;
        private float last = 0;

        private float dt = 0;
        private float dtMs = 0;

        /// <summary>
        /// the how manieth frame this is since simulation start
        /// </summary>
        public int Frame
        {
            get { return frame; }
        }

        /// <summary>
        /// current time in total milliseconds passed since simulation start
        /// </summary>
        public float At
        {
            get { return at; }
        }

        /// <summary>
        /// last time in total milliseconds passed since simulation start
        /// </summary>
        public float Last
        {
            get { return last; }
        }

        /// <summary>
        /// difference between last and current update in fraction of a second
        /// </summary>
        public float Dt
        {
            get { return dt; }
        }

        /// <summary>
        /// difference between last and current update in milliseconds
        /// </summary>
        public float DtMs
        {
            get { return dtMs; }
        }

        internal void Update()
        {
            // reset last
            last = at;

            // dt in milliseconds
            dtMs = (float)((DateTime.Now.Ticks - lastTick) / 10000d);

            // dt in seconds
            dt = dtMs / 1000f;

            // at is in milliseconds
            at += dtMs;

            // increase frame counter
            frame++;

            // reset lastTick
            lastTick = DateTime.Now.Ticks;
        }

        internal void Pause()
        {
            lastTick = DateTime.Now.Ticks;
        }

        public static float GetDt(float from, float to)
        {
            return (to - from) / 1000f;
        }
    }
}
