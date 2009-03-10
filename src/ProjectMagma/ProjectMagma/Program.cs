using System;
using System.Threading;

namespace ProjectMagma
{
    static class Program
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

            // another thing to clarify would be on which thread the Main-methode is running (
            // and if the SetProcessorAffinity works... It could very well be that this thread is
            // already locked to some hardware thread.
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 1 });
#endif

            using (Game1 game = new Game1())
            {
                game.Run();
            }
        }
    }
}

