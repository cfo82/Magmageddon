using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma
{
    public class GlobalClock
    {
        public GlobalClock()
        {
        	this.running = false;
        	this.paused = false;
        	this.continuousStartTick = this.pausableStartTick = 0;
        	this.pauseTick = 0;
        }

		public void Start()
		{
			lock (this)
			{
				Debug.Assert(!this.paused);
				Debug.Assert(!this.running);
				this.running = true;
                this.paused = true;
				this.continuousStartTick = this.pausableStartTick = this.pauseTick = DateTime.Now.Ticks;
			}
		}
		
		public void Pause()
		{
			lock (this)
			{
				Debug.Assert(!this.paused);
				this.paused = true;
				this.pauseTick = DateTime.Now.Ticks;
			}
		}
		
		public void Resume()
		{
			lock (this)
			{
				Debug.Assert(this.paused);
				long resumeTick = DateTime.Now.Ticks;
				long tickDiff = resumeTick - this.pauseTick;
				this.pausableStartTick += tickDiff;
				this.paused = false;
			}
		}
		
		public void Stop()
		{
			lock (this)
			{
				this.running = false;
			}
		}
		
		private double CalculateMilliseconds(
			long nowTick,
			long startTick
		)
		{
			long nowTime = nowTick - startTick;
			return nowTime / 10000d;
		}
		
		public double ContinuousMilliseconds
		{
			get
			{
				lock (this)
				{
					Debug.Assert(this.running);
					return CalculateMilliseconds(DateTime.Now.Ticks, this.continuousStartTick);
				}
	        }
		}
		
		public double ContinuousSeconds
		{
			get
			{
				return ContinuousMilliseconds / 1000d;
	        }
		}
		
		public double PausableMilliseconds
		{
			get
			{
				lock (this)
				{
					Debug.Assert(this.running);
					return CalculateMilliseconds(
						this.paused ? this.pauseTick : DateTime.Now.Ticks,
						this.pausableStartTick
					);
				}
	        }
		}
		
		public double PausableSeconds
		{
			get
			{
				return PausableMilliseconds / 1000d;
	        }
		}
		
		private bool running;
		private bool paused;
        private long continuousStartTick;
        private long pausableStartTick;
        private long pauseTick;
    }
}
