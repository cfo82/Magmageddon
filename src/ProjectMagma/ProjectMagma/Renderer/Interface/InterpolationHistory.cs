using System.Diagnostics;

namespace ProjectMagma.Renderer.Interface
{
    public abstract class InterpolationHistory<ValueType>
    {
        struct TimeValuePair
        {
            public double Time;
            public ValueType Value;
        }

        public InterpolationHistory(double startTimestamp, ValueType startValue)
        {
            history = new TimeValuePair[arraySize];
            currentIndex = 0;
            nextIndex = 1;
            history[currentIndex].Time = startTimestamp;
            history[currentIndex].Value = startValue;
            cacheValid = false;
        }

        public void AddKeyframe(double time, ValueType value)
        {
            cacheValid = false;
            history[nextIndex].Time = time;
            history[nextIndex].Value = value;
            nextIndex = IncrementIndex(nextIndex);
            if (nextIndex == currentIndex)
            {
                currentIndex = IncrementIndex(currentIndex);
            }
        }

        public void InvalidateUntil(double time)
        {
            int nextCurrentIndex = IncrementIndex(currentIndex);
            while (nextCurrentIndex != nextIndex && history[nextCurrentIndex].Time < time)
            { 
                currentIndex = nextCurrentIndex;
                nextCurrentIndex = IncrementIndex(currentIndex);
            }
        }

        public abstract void Interpolate(ref ValueType valFrom, ref ValueType valTo, float amount, out ValueType returnValue);

        public ValueType Evaluate(double time)
        {
            // test for a cached value
            if (cacheValid && cachedTime == time)
            {
                return cachedValue;
            }


            // time below recorded history...
            if (time < history[currentIndex].Time)
            {
                //Console.WriteLine("{0} < {1} -> early exit", time, history[currentIndex].Time);
                
                cacheValid = true;
                cachedTime = time;
                cachedValue = history[currentIndex].Value;

                return history[currentIndex].Value;
            }
            
            // search forward until we find the correct time
            int fromKeyframe = currentIndex;
            int untilKeyframe = IncrementIndex(fromKeyframe);
            while (untilKeyframe != nextIndex && history[untilKeyframe].Time <= time)
            {
                fromKeyframe = untilKeyframe;
                untilKeyframe = IncrementIndex(fromKeyframe);
            }

            // test if time above recorded history...
            if (untilKeyframe == nextIndex)
            {
                //Console.WriteLine("{0} > {1} -> early exit", time, history[fromKeyframe].Time);

                cacheValid = true;
                cachedTime = time;
                cachedValue = history[fromKeyframe].Value;

                return history[fromKeyframe].Value;
            }

            // assert and evaluate the remaining case...
            Debug.Assert(history[fromKeyframe].Time <= time);
            Debug.Assert(history[untilKeyframe].Time > time);
            Debug.Assert(IncrementIndex(fromKeyframe) == untilKeyframe);

            cacheValid = true;
            cachedTime = time;
            Interpolate(
                ref history[fromKeyframe].Value,
                ref history[untilKeyframe].Value,
                (float)((time - history[fromKeyframe].Time) / (history[untilKeyframe].Time - history[fromKeyframe].Time)),
                out cachedValue);

            //Console.WriteLine("interpolated time {0} between {1} and {2} resulted in {3}->{4}->{5}",
            //    time, history[fromKeyframe].Time, history[untilKeyframe].Time,
            //    history[fromKeyframe].Value, cachedValue, history[untilKeyframe].Value);

            return cachedValue;
        }

        public int IncrementIndex(int index)
        {
            return (index + 1) % arraySize;
        }

        private static readonly int arraySize = 200;
        private TimeValuePair[] history;
        private int currentIndex;
        private int nextIndex;
        private bool cacheValid;
        private double cachedTime;
        private ValueType cachedValue;
    }
}
