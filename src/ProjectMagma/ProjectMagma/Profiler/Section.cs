using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Profiler
{
    class Section
    {
        private static readonly int NumFrameStatistics = 20;

        public Section(
            Profiler profiler,
            Section parent,
            string name
        )
        {
            this.profiler = profiler;
            this.parent = parent;
            this.name = name;
            this.fullName = String.Format("{0}{1}", (parent != null ? string.Format("{0}.", parent.FullName) : ""), name);
            this.allocatedAtFrame = profiler.FrameNumber;
            this.childrenMap = new Dictionary<string, Section>();
            this.childrenList = new List<Section>();
            frameStatistics = new FrameStatistics[NumFrameStatistics];
            for (int i = 0; i < NumFrameStatistics; ++i)
            {
                frameStatistics[i].Initialized = false;
            }
            currentStatistics = 0;
            totalStatistics = new FrameStatistics();
            totalFrameCount = 0;
            peakStatistics = new FrameStatistics();
        }

        public Section AllocateChild(string name)
        {
            if (childrenMap.ContainsKey(name))
            {
                throw new Exception(string.Format("a sub-section with name {0} exists already!", name));
            }

            Section newSection = new Section(this.profiler, this, name);
            childrenMap.Add(name, newSection);
            childrenList.Add(newSection);
            return newSection;
        }

        public void BeginFrame()
        {
            foreach (Section child in childrenMap.Values)
            {
                child.BeginFrame();
            }

            frameStatistics[currentStatistics].Clear();
        }

        public void BeginSection()
        {
            Debug.Assert(!frameStatistics[currentStatistics].InSection);

            frameStatistics[currentStatistics].Initialized = true;
            ++frameStatistics[currentStatistics].CallCount;
            frameStatistics[currentStatistics].InSection = true;
            frameStatistics[currentStatistics].FrameId = this.profiler.FrameNumber;
            frameStatistics[currentStatistics].LastSectionStart = Now();
        }

        public void EndSection()
        {
            double time = Now();
            frameStatistics[currentStatistics].AccumulatedTime += time - frameStatistics[currentStatistics].LastSectionStart;
            frameStatistics[currentStatistics].InSection = false;
        }

        public void EndFrame()
        {
            if (profiler.FrameNumber != this.allocatedAtFrame)
            {
                totalStatistics.AccumulatedTime += frameStatistics[currentStatistics].AccumulatedTime;
                totalStatistics.CallCount += frameStatistics[currentStatistics].CallCount;
                ++totalFrameCount;

                if (frameStatistics[currentStatistics].AccumulatedTime > peakStatistics.AccumulatedTime)
                    { peakStatistics.AccumulatedTime = frameStatistics[currentStatistics].AccumulatedTime; }
                if (frameStatistics[currentStatistics].CallCount > peakStatistics.CallCount)
                   { peakStatistics.CallCount = frameStatistics[currentStatistics].CallCount; }
            }

            currentStatistics = (currentStatistics + 1) % NumFrameStatistics;
            
            foreach (Section child in childrenMap.Values)
            {
                child.EndFrame();
            }
        }

        public Section this[string name]
        {
            get
            {
                Section value;
                if (childrenMap.TryGetValue(name, out value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
        }

        public Section this[int index]
        {
            get
            {
                return childrenList[index];
            }
        }

        public int ChildCount
        {
            get { return childrenList.Count; }
        }

        public string Name
        {
            get { return name; }
        }

        private double Now()
        {
            double now = DateTime.Now.Ticks;
            double ms = now / 10000d;
            double s = ms / 1000d;
            return s;
        }

        public void WriteFrame(StreamWriter writer, int indent, long frameNumber)
        {
            bool hasStats = false;
            FrameStatistics stats = new FrameStatistics();
            foreach (FrameStatistics i in frameStatistics)
            {
                if (i.Initialized && i.FrameId == frameNumber)
                {
                    stats = i;
                    hasStats = true;
                }
            }

            if (!hasStats)
            {
                writer.WriteLine("{0}+-- {1} has no statistics", GenerateIndent(indent), name);
            }
            else
            {
                writer.WriteLine("{0}+-- {1}: {2}, {3}", GenerateIndent(indent), name, stats.CallCount, stats.AccumulatedTime);
            }

            foreach (Section child in childrenMap.Values)
            {
                child.WriteFrame(writer, indent + 1, frameNumber);
            }
        }

        public void WriteGeneral(StreamWriter writer, int indent)
        {
            string indentString = GenerateIndent(indent);

            writer.WriteLine("{0}+-- {1}:", indentString, name);
            writer.WriteLine("{0}      Peak CallCount:    {1}", indentString, peakStatistics.CallCount);
            writer.WriteLine("{0}      Peak Time:         {1}", indentString, peakStatistics.AccumulatedTime);

            writer.WriteLine("{0}      Total CallCount:   {1}", indentString, totalStatistics.CallCount);
            writer.WriteLine("{0}      Total Time:        {1}", indentString, totalStatistics.AccumulatedTime);

            writer.WriteLine("{0}      Average CallCount: {1}", indentString, (double)totalStatistics.CallCount / (double)totalFrameCount);
            writer.WriteLine("{0}      Average Time:      {1}", indentString, (double)totalStatistics.AccumulatedTime / (double)totalFrameCount);

            foreach (Section child in childrenMap.Values)
            {
                child.WriteGeneral(writer, indent + 1);
            }
        }

        private string GenerateIndent(int indent)
        {
            string val = "";
            for (int i = 0; i < indent; ++i)
                { val += "  "; }
            return val;
        }

        public int TotalFrameCount
        {
            get { return totalFrameCount; }
        }

        public FrameStatistics TotalStatistics
        {
            get { return totalStatistics; }
        }

        public FrameStatistics PeakStatistics
        {
            get { return peakStatistics; }
        }

        public FrameStatistics CurrentStatistics
        {
            get
            {
                FrameStatistics stats = new FrameStatistics();
                for (int i = 0; i < frameStatistics.Length; ++i)
                {
                    if (frameStatistics[i].Initialized)
                    {
                        stats.CallCount += frameStatistics[i].CallCount;
                        stats.AccumulatedTime += frameStatistics[i].AccumulatedTime;
                    }
                }
                return stats;
            }
        }

        public string FullName
        {
            get { return fullName; }
        }

        private Profiler profiler;
        private Section parent;
        private string name;
        private string fullName;
        private long allocatedAtFrame;
        private Dictionary<string, Section> childrenMap;
        private List<Section> childrenList;
        private FrameStatistics[] frameStatistics;
        private int currentStatistics;
        private FrameStatistics totalStatistics;
        private int totalFrameCount;
        private FrameStatistics peakStatistics;
    }
}
