using System;

namespace Beast.Claw.Common
{
    public static class BeastUtil
    {
        private static class EnumStorage<T> where T : Enum
        {
            public static readonly T[] Values = (T[]) Enum.GetValues(typeof(T));
        }
        
        public static T[] GetEnumValues<T>() where T : Enum
        {
            return EnumStorage<T>.Values;
        }
    }
}