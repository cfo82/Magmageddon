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
            this.profiler = ProjectMagma.Profiler.Profiler.CreateProfiler(null, "simulation_profiler"); 
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

        public double Sps;
        public double AvgSps;

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
                        profiler.BeginFrame();
                        RendererUpdateQueue q = simulation.Update();
                        renderer.AddUpdateQueue(q);

                        Sps = 1000f / simulation.Time.DtMs;
                        AvgSps = 1000f * simulation.Time.Frame / simulation.Time.At;
                        profiler.EndFrame();
                    }

                    finishedEvent.Set();
                }
            }
#if DEBUG
            catch (ThreadAbortException ex)
            {
                if (!this.aborted)
                {
                    System.Console.WriteLine("unexpected Exception {0}\n{1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
                    throw ex;
                }
            }
#else
            catch (Exception ex)
            {
                if (!this.aborted)
                {
                    System.Console.WriteLine("unexpected Exception {0}\n{1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
                    Game.Instance.CrashDebugger.Crash(ex);
                    finishedEvent.Set();
                }
            } 
#endif
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
            this.thread.Join();
        }

        public Thread Thread
        {
            get { return thread; }
        }

        public Profiler.Profiler Profiler
        {
            get
            {
                return this.profiler;
            }
        }

        private Simulation simulation;
        private Renderer.Renderer renderer;
        private AutoResetEvent startEvent;
        private AutoResetEvent finishedEvent;
        private volatile bool aborted;
        private static readonly int processor = 1;
        private Thread thread;
        private volatile bool joinRequested;
        private Profiler.Profiler profiler;
    }
}
