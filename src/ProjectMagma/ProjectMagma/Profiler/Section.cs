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
            this.allocatedAtFrame = profiler.FrameNumber;
            this.children = new Dictionary<string, Section>();
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
            if (children.ContainsKey(name))
            {
                throw new Exception(string.Format("a sub-section with name {0} exists already!", name));
            }

            Section newSection = new Section(this.profiler, this, name);
            children.Add(name, newSection);
            return newSection;
        }

        public void BeginFrame()
        {
            foreach (Section child in children.Values)
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
            
            foreach (Section child in children.Values)
            {
                child.EndFrame();
            }
        }

        public Section this[string name]
        {
            get
            {
                Section value;
                if (children.TryGetValue(name, out value))
                {
                    return value;
                }
                else
                {
                    return null;
                }
            }
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
            string indentString = GenerateIndent(indent);

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
                writer.WriteLine("{0}+-- {1} has no statistics", indentString, name);
            }
            else
            {
                writer.WriteLine("{0}+-- {1}:", indentString, name);
                writer.WriteLine("{0}      CallCount: {1}", indentString, stats.CallCount);
                writer.WriteLine("{0}      Time:      {1}", indentString, stats.AccumulatedTime);
            }

            foreach (Section child in children.Values)
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

            foreach (Section child in children.Values)
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

        private Profiler profiler;
        private Section parent;
        private string name;
        private long allocatedAtFrame;
        private Dictionary<string, Section> children;
        private FrameStatistics[] frameStatistics;
        private int currentStatistics;
        private FrameStatistics totalStatistics;
        private int totalFrameCount;
        private FrameStatistics peakStatistics;
    }
}
