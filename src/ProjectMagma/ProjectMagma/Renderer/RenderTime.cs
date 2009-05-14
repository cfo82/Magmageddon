using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Renderer
{
    public class RenderTime
    {
        private int frame = 0;

        private double at = 0;
        private double last = 0;
        private double dt = 0;
        private double dtMs = 0;

		private double pausableAt = 0;
		private double pausableLast = 0;
		private double pausableDt = 0;
		private double pausableDtMs = 0;

        public RenderTime(double at, double pausableAt)
        {
            this.last = this.at = at;
            this.pausableAt = this.pausableLast = pausableAt;
        }

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
        public double At
        {
            get { return at; }
        }

        /// <summary>
        /// last time in total milliseconds passed since simulation start
        /// </summary>
        public double Last
        {
            get { return last; }
        }

        /// <summary>
        /// difference between last and current update in fraction of a second
        /// </summary>
        public double Dt
        {
            get { return dt; }
        }

        /// <summary>
        /// difference between last and current update in milliseconds
        /// </summary>
        public double DtMs
        {
            get { return dtMs; }
        }
        
        public double PausableAt
        {
        	get { return pausableAt; }
        }
        
        public double PausableLast
        {
        	get { return pausableLast; }
        }
        
        public double PausableDt
        {
        	get { return pausableDt; }
        }
        
        public double PausableDtMs
        {
        	get { return pausableDtMs; }
        }

        internal void Update()
        {
            // reset last
            last = at;
			at = Game.Instance.GlobalClock.ContinuousMilliseconds;
            dtMs = at - last;
            dt = dtMs / 1000d;
			// reset pausable last
			pausableLast = pausableAt;
			pausableAt = Game.Instance.GlobalClock.PausableMilliseconds;
			pausableDtMs = pausableAt - pausableLast;
			pausableDt = pausableDtMs / 1000d;
            // increase frame counter
            ++frame;
        }

        public static double GetDt(double from, double to)
        {
            return (to - from) / 1000d;
        }
    }
}
