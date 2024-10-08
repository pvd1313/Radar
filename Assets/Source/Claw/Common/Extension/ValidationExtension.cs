using System;
using System.Runtime.CompilerServices;

namespace Beast.Claw.Common
{
    public static class ValidationExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Validate<T>(this T data) where T : IValidatable
        {
            if (!data.IsValid(out string message))
            {
                throw new InvalidOperationException(message);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotNull<T>(this T obj, string objName) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentException($"{objName} can not be null");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotEmpty(this string text, string objName)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException($"{objName} can not be null or empty");
            }
        }
    }
}