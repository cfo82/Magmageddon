using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMagma.Bugslayer;

namespace ProjectMagma
{
    class EntryPoint
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            // we can use hardware threads 1, 3, 4, 5
            //  1 is running on core 1 alongside something reserved to xna
            //  3 same thing on core 2
            //  4, 5 run on core 3 and are freely available for us

            // I am not sure if we have to do the rendering using the thread that created
            // the direct3d device. This would be something to clarify in the future. As for
            // now we should keep to rendering on the main thread and move everything else to
            // the other cores.
#if XBOX
            System.Threading.Thread.CurrentThread.SetProcessorAffinity(ThreadDistribution.RenderThread);

            try
            {
#endif
                Game.RunInstance();

#if XBOX
            }
            catch (Exception exception)
            {
                using (CrashDebugGame game = new CrashDebugGame(exception))
                {
                    game.Run();
                }
            }
#endif
        }

    }
}