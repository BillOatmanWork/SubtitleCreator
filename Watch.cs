using System;
using System.Diagnostics;
using System.Globalization;

namespace SubtitleCreator
{
    /// <summary>
    /// Convenience wrapper for Stopwatch to make it easier to time things.
    /// </summary>
    public static class Watch
    {
        private static readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// Start the watch.  Always resets back to zero.
        /// </summary>
        public static void WatchStart()
        {
            stopwatch.Restart();
        }

        // ... other code ...

        /// <summary>
        /// Stop the watch and return a elapsed time string.
        /// </summary>
        /// <returns></returns>
        public static string WatchStop()
        {
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:00}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10);
        }
    }
}
