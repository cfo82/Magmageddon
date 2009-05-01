using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class SimulationThread
    {
        public SimulationThread(
            Simulation simulation,
            Renderer.Renderer renderer
            )
        {
            this.simulation = simulation;
            this.renderer = renderer;

            this.startEvent = new AutoResetEvent(false);
            this.finishedEvent = new AutoResetEvent(false);
            this.aborted = false;

            this.thread = new Thread(Run);
            this.thread.Name = "SimulationThread" + processor;
            this.thread.Start();
        }

        public void Start()
        {
            joinRequested = false;
            startEvent.Set();
        }

        private double Now
        {
            get
            {
                double now = DateTime.Now.Ticks;
                double ms = now / 10000d;
                return ms;
            }
        }

        public double Sps;
        public double AvgSps;

        private void Run()
        {
#if XBOX
            this.thread.SetProcessorAffinity(processor);
#endif
            try
            {
                while (true)
                {
                    startEvent.WaitOne();

                    double numFrames = 0;
                    double time = 0;
                    double current = Now;
                    while (!joinRequested)
                    {
                        RendererUpdateQueue q = simulation.Update();
                        renderer.AddUpdateQueue(q);

                        double now = Now;
                        double dt = now - current;

                        time += dt;
                        if (dt > 0)
                        {
                            Sps = (float)(1000f / dt);
                        }
                        if (time > 0)
                        {
                            AvgSps = (1000.0 * numFrames / time);
                        }

                        current = now;
                        ++numFrames;
                    }

                    finishedEvent.Set();
                }
            }
            catch (ThreadAbortException ex)
            {
                if (!this.aborted)
                {
                    System.Console.WriteLine("unexpected ThreadAbortException {0}", ex);
                    throw ex;
                }
            }
        }

        public void Join()
        {
            joinRequested = true;
            finishedEvent.WaitOne();
        }

        public void Abort()
        {
            this.aborted = true;
            this.thread.Abort();
        }

        private Simulation simulation;
        private Renderer.Renderer renderer;
        private AutoResetEvent startEvent;
        private AutoResetEvent finishedEvent;
        private volatile bool aborted;
        private static readonly int processor = 1;
        private Thread thread;
        private volatile bool joinRequested;
    }
}
