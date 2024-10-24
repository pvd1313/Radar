using System;
using Unity.Collections;

namespace Beast.Claw.Profiling
{
    public struct ProfilerTree : IDisposable
    {
        public string[] text;
        public NativeArray<ushort> childs;
        public NativeArray<ushort> siblings;
        public NativeArray<ushort> names;
        public NativeArray<float> times;

        public void Dispose()
        {
            childs.Dispose();
            siblings.Dispose();
            names.Dispose();
            times.Dispose();
        }
    }
}