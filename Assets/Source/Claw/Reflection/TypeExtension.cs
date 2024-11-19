using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Beast.Claw.Reflection
{
    public static class TypeExtension
    {
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TypeFromAssembly<TAssembly>(this string type, ref Type cash)
        {
            BReflect.TypeFromAssembly<TAssembly>(type, ref cash);
        }
        
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type TypeFromAssembly<TAssembly>(this string type)
        {
            return BReflect.TypeFromAssembly<TAssembly>(type);
        }

        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo MethodFromType(this string method, Type type, BindingFlags flags)
        {
            return BReflect.MethodFromType(type, method, flags);
        }
        
        /// <exception cref="Exception"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MethodFromType(this string method, Type type, BindingFlags flags, ref MethodInfo cash)
        {
            BReflect.MethodFromType(type, method, flags, ref cash);
        }
        
        /// <see cref="Type.IsAssignableFrom"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Implements(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        /// <summary>
        /// System.Type.IsDefined(inherit: true);
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute(this Type type, Type attributeType)
        {
            return type.IsDefined(attributeType, true);
        }
    }
}