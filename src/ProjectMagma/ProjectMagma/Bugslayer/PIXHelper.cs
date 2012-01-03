using System.Runtime.InteropServices;
using System.Diagnostics;

using Microsoft.Xna.Framework;


namespace ProjectMagma.Bugslayer
{
#if XBOX
    public static class PIXHelper
    {
        public static void BeginEvent(string eventName)
        {
        }

        public static void EndEvent()
        {
        }
    }
#else
    /// <summary>
    /// Contains static methods for creating events in markers
    /// used during PIX profiling.
    /// </summary>
    public static class PIXHelper
    {
        #region private fields

        // Keeps track of our BeginEvent/EndEvent pairs
        private static int eventCount = 0;

        #endregion

        #region Public static methods

        /// <summary>
        /// Determines whether PIX is attached to the running process
        /// </summary>
        /// <returns>true if PIX is attached, false otherwise</returns>
        public static bool IsPIXAttached()
        {
            return (D3DPERF_GetStatus() > 0);
        }

        /// <summary>
        /// Calling this method will prevent PIX from being able
        /// to attach to this process.
        /// </summary>
        public static void DisablePIXProfiling()
        {
            D3DPERF_SetOptions(1);
        }

        /// <summary>
        /// Marks the beginning of a PIX event.  Events are used to group
        /// together a series of related API calls, for example a series of
        /// commands needed to draw a single model.  Events can be nested.
        /// </summary>
        /// <param name="eventName">The name of the event</param>
        public static void BeginEvent(string eventName)
        {
            D3DPERF_BeginEvent(Color.Black.PackedValue, eventName);
            eventCount++;
        }

        /// <summary>
        /// Marks the end of a PIX event.
        /// </summary>
        public static void EndEvent()
        {
            // Make sure we haven't called EndEvent more times
            // than we've called BeginEvent
            Debug.Assert(eventCount >= 1);

            D3DPERF_EndEvent();
            eventCount--;
        }

        /// <summary>
        /// Adds a marker in PIX.  A marker is used to indicate that single
        /// instantaneous event has occured.
        /// </summary>
        /// <param name="markerName"></param>
        public static void SetMarker(string markerName)
        {
            D3DPERF_SetMarker(Color.Black.PackedValue, markerName);
        }

        #endregion

        #region PInvokes

        [DllImport("d3d9.dll")]
        private static extern uint D3DPERF_GetStatus();

        [DllImport("d3d9.dll")]
        private static extern void D3DPERF_SetOptions(uint dwOptions);

        [DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
        private static extern int D3DPERF_BeginEvent(uint col, string wszName);

        [DllImport("d3d9.dll")]
        private static extern int D3DPERF_EndEvent();

        [DllImport("d3d9.dll", CharSet = CharSet.Unicode)]
        private static extern void D3DPERF_SetMarker(uint col, string wszName);

        #endregion
    }
#endif
}
