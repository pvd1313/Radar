using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;

namespace Beast.Claw.Reflection
{
    internal static class EnumStorage<T> where T : Enum
    {
        public static readonly T[] Values;

        static EnumStorage()
        {
            try
            {
                Values = (T[])Enum.GetValues(typeof(T));
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
            }
        }
    }
}