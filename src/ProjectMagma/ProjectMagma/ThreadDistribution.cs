using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma
{
    public class ThreadDistribution
    {
        public static readonly int RenderThread = 4;
        public static readonly int SimulationThread = 1;
        public static readonly int[] CollisionThreads = { 1, 3 };
    }
}
