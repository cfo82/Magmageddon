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
            startEvent.Set();
        }

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

                    RendererUpdateQueue q = simulation.Update();
                    renderer.AddUpdateQueue(q);

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
        private bool aborted;
        private static readonly int processor = 1;
        private Thread thread;
    }
}
