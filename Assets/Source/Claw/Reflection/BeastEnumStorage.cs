using System;
using Beast.Claw.Common;

namespace Beast.Claw.Reflection
{
    internal static class BeastEnumStorage<T> where T : Enum
    {
        public static readonly T[] Values;

        static BeastEnumStorage()
        {
            try
            {
                Values = (T[])Enum.GetValues(typeof(T));
            }
            catch (Exception e)
            {
                BDebug.LogStaticException(e, "BeastEnumStorage<T> cctor");
            }
        }
    }
}