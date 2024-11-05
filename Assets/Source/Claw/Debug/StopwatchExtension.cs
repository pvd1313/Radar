using System.Diagnostics;

namespace Beast.Claw.Debug
{
    public static class StopwatchExtension
    {
        public static float ElapsedMillisecondsFloat(this Stopwatch stopwatch)
        {
            return stopwatch.ElapsedTicks / (float)Stopwatch.Frequency * 1000.0f;
        }
    }
}