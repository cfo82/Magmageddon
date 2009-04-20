using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma.Profiler
{
    public class Profiler
    {
        public Profiler()
        {
            rootSection = new Section(this, null, "root");
            sectionStack = new List<Section>(100);
            frameNumber = 0;
        }

        public static Profiler CreateProfiler(string debuginfo)
        {
            Console.WriteLine("profiling code using {0}!", debuginfo);
            return new Profiler();
        }

        [Conditional("PROFILING")]
        public void BeginFrame()
        {
            rootSection.BeginFrame();

            sectionStack.Clear();
            sectionStack.Add(rootSection);
        }

        [Conditional("PROFILING")]
        public void BeginSection(string name)
        {
            Section parent = sectionStack[sectionStack.Count - 1];
            Section child = parent[name];
            if (child == null)
            {
                child = parent.AllocateChild(name);
            }

            sectionStack.Add(child);
            child.BeginSection();
        }

        [Conditional("PROFILING")]
        public void EndSection(string name)
        {
            Section child = sectionStack[sectionStack.Count - 1];
            Debug.Assert(child.Name == name);
            child.EndSection();

            sectionStack.RemoveAt(sectionStack.Count - 1);
        }

        [Conditional("PROFILING")]
        public void EndFrame()
        {
            Debug.Assert(sectionStack.Count == 1);
            rootSection.EndFrame();
            ++frameNumber;
        }

        public long FrameNumber
        {
            get { return frameNumber; }
        }

        public void Write(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Truncate, FileAccess.Write, FileShare.Write);
            fs.Close();

            StreamWriter sw = new StreamWriter(filename, true, Encoding.ASCII);
            Write(sw);
            sw.Close();
        }

        public void Write(StreamWriter writer)
        {
            writer.WriteLine("profiling information:");
            writer.WriteLine("----------------------");
            writer.WriteLine("");
            writer.WriteLine("Frame Information:");
            for (long i = 0; i < frameNumber; ++i)
            {
                writer.WriteLine("  Frame {0}:", i);
                rootSection.WriteFrame(writer, 2, i);
            }

            writer.WriteLine("General Information:");
            rootSection.WriteGeneral(writer, 1);
        }

        private Section rootSection;
        private List<Section> sectionStack;
        long frameNumber;
    }
}
