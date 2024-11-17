using System;
using Beast.Claw.Common;
using Unity.Collections;

namespace Beast.Claw.Profiling
{
    public struct ProfilerTree : IValidatable, IDisposable
    {
        public string[] text;
        public NativeArray<ushort> childs;
        public NativeArray<ushort> siblings;
        public NativeArray<ushort> names;
        public NativeArray<float> times;
        public int nodeCount;
        
        public string Validate()
        {
            if (text == null)
            {
                return "text is null";
            }

            if (!childs.IsCreated)
            {
                return "childs is not created";
            }

            if (!siblings.IsCreated)
            {
                return "simblings is not created";
            }

            if (!names.IsCreated)
            {
                return "names is not created";
            }

            if (!times.IsCreated)
            {
                return "times is not created";
            }

            if (nodeCount <= 0)
            {
                return "invalid node count: " + nodeCount;
            }

            return null;
        }

        public void Dispose()
        {
            childs.Dispose();
            siblings.Dispose();
            names.Dispose();
            times.Dispose();
        }
    }
}