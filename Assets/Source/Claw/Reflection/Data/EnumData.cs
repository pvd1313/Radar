using System;
using Unity.Collections;

namespace Beast.Claw.Reflection
{
    public struct EnumData
    {
        public bool isFlags;
        public long flagsMask;
        public TypeCode typeCode;
        public NativeHashSet<long> allValues;
    }
}