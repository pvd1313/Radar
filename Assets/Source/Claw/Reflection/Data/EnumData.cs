using System;
using System.Collections.Generic;

namespace Beast.Claw.Reflection
{
    public struct EnumData
    {
        public bool isFlags;
        public long flagsMask;
        public TypeCode typeCode;
        public HashSet<long> allValues;
    }
}