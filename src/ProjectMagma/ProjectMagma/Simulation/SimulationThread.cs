﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using ProjectMagma.Renderer.Interface;

namespace ProjectMagma.Simulation
{
    public class SimulationThread
    {
        public SimulationThread()
        {
            this.startEvent = new AutoResetEvent(false);
            this.finishedEvent = new AutoResetEvent(false);
            this.aborted = false;

            //this.contentManager = new ContentManager(Game.Instance.Services);
            //this.contentManager.RootDirectory = "Content";

            this.thread = null;
        }

        public void Reinitialize(
            Simulation simulation,
            Renderer.Renderer renderer
        )
        {
            this.simulation = simulation;
            this.renderer = renderer;

            if (this.thread == null)
            {
                this.thread = new Thread(Run);
                this.thread.Name = "SimulationThread" + processor;
                this.thread.Start();
            }
        }

        public void Start()
        {
            joinRequested = false;
            startEvent.Set();
        }

        public volatile double Sps;
        public volatile double AvgSps;

        private void Run()
        {
#if XBOX
            this.thread.SetProcessorAffinity(ThreadDistribution.SimulationThread);
#endif
            try
            {
                while (true)
                {
                    startEvent.WaitOne();

                    while (!joinRequested)
                    {
                        RendererUpdateQueue q = simulation.Update();
                        renderer.AddUpdateQueue(q);

                        Sps = 1000f / simulation.Time.DtMs;
                        AvgSps = 1000f * simulation.Time.Frame / simulation.Time.At;
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

        public Thread Thread
        {
            get { return thread; }
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
