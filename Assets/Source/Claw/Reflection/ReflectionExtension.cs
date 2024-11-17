using System;
using System.Runtime.CompilerServices;

namespace Beast.Claw.Reflection
{
    public static class ReflectionExtension
    {
        /// <see cref="Type.IsAssignableFrom"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Implements(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }
    }
}