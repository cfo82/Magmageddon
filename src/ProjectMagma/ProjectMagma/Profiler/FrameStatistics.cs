using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Profiler
{
    struct FrameStatistics
    {
        public void Clear()
        {
            Initialized = true;
            CallCount = 0;
            InSection = false;
            LastSectionStart = 0.0;
            AccumulatedTime = 0.0;
            FrameId = 0;
        }

        public bool Initialized;
        public int CallCount;
        public bool InSection;
        public double LastSectionStart;
        public double AccumulatedTime;
        public long FrameId;
    }
}