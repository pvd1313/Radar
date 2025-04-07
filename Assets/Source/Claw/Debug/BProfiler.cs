using System.Diagnostics;
using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using UnityEngine.Profiling;

namespace Beast.Claw.Debug
{
    [StaticUtility]
    public static class BProfiler
    {
        public static bool UnityProfilerActive
        {
            get
            {
#if UNITY_EDITOR
                if (UnityEditorInternal.ProfilerDriver.enabled)
                {
                    return true;
                }
#endif

                return Profiler.enabled;
            }
        }

        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BeginSample(string sampleName)
        {
            Profiler.BeginSample(sampleName);
        }

        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EndSample()
        {
            Profiler.EndSample();
        }
    }
}