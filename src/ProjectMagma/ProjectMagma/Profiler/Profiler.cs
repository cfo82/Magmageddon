using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Storage;

namespace ProjectMagma.Profiler
{
    public class Profiler
    {
        public Profiler()
        {
            rootSection = new Section(this, null, "root");
            sectionStack = new List<Section>(100);
            frameNumber = 0;
            inBeginFrame = false;
        }

        public static Profiler CreateProfiler(string debuginfo)
        {
            Console.WriteLine("profiling code using {0}!", debuginfo);
            return new Profiler();
        }

        [Conditional("PROFILING")]
        public void BeginFrame()
        {
            inBeginFrame = true;
            rootSection.BeginFrame();

            sectionStack.Clear();
            sectionStack.Add(rootSection);
            rootSection.BeginSection();
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
            rootSection.EndSection();
            rootSection.EndFrame();
            ++frameNumber;
            inBeginFrame = false;
        }

        [Conditional("PROFILING")]
        public void TryEndFrame()
        {
            if (inBeginFrame)
            {
                EndFrame();
            }
        }

        [Conditional("PROFILING")]
        public void TryBeginFrame()
        {
            if (!inBeginFrame)
            {
                BeginFrame();
            }
        }

        public long FrameNumber
        {
            get { return frameNumber; }
        }

        [Conditional("PROFILING")]
        public void Write(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write);
            fs.Close();

            StreamWriter sw = new StreamWriter(filename, true, Encoding.ASCII);
            Write(sw);
            sw.Close();
        }

        [Conditional("PROFILING")]
        public void Write(StorageDevice device, string windowTitle, string filename)
        {
            // Open a storage container.StorageContainer container =
            StorageContainer container = device.OpenContainer(windowTitle);

            // Get the path of the save game.
            string absoluteFilename = Path.Combine(container.Path, filename);

            // Open the file, creating it if necessary.
            using (FileStream stream = File.Open(absoluteFilename, FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(stream);
                Write(writer);
                writer.Close();
            }

            // Dispose the container, to commit changes.
            container.Dispose();
        }

        [Conditional("PROFILING")]
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
        bool inBeginFrame;
    }
}
