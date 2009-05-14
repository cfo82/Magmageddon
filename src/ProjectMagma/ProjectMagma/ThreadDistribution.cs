using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectMagma
{
    public class ThreadDistribution
    {
        public static readonly int RenderThread = 1;
        public static readonly int SimulationThread = 3;
        public static readonly int[] CollisionThreads = { 3, 4 };
    }
}
