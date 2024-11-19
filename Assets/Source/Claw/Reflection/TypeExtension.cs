using System;
using System.Runtime.CompilerServices;

namespace Beast.Claw.Reflection
{
    public static class TypeExtension
    {
        /// <see cref="Type.IsAssignableFrom"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Implements(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        /// <summary>
        /// System.Type.IsDefined(inherit: true);
        /// </summary>
        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return type.IsDefined(attributeType, true);
        }
    }
}