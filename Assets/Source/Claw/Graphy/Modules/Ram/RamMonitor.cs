using UnityEngine.Profiling;

namespace Beast.Claw.Graphy.Modules.Ram
{
    public class RamMonitor : GraphyMonitor
    {
        public float AllocatedRam { get; private set; }
        public float ReservedRam { get; private set; }
        public float MonoRam { get; private set; }

        public override void Update()
        {
            if (!IsEnabled)
            {
                return;
            }
            
            AllocatedRam = Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            ReservedRam = Profiler.GetTotalReservedMemoryLong() / 1048576f;
            MonoRam = Profiler.GetMonoUsedSizeLong() / 1048576f;
        }
    }
}