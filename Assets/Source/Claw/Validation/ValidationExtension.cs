using System;
using System.Runtime.CompilerServices;

namespace Beast.Claw
{
    public static class ValidationExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateThrowException<T>(this T data) where T : IValidatable
        {
            string errorMessage = data.Validate();
            if (errorMessage != null)
            {
                throw new ValidationException(errorMessage);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid<T>(this T data, out string errorMessage) where T : IValidatable
        {
            errorMessage = data.Validate();

            return errorMessage == null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotNullThrowException<T>(this T obj, string objName)
        {
            if (obj == null)
            {
                throw new ValidationException($"{objName} can not be null");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotEmptyOrNullThrowException(this string text, string objName)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ValidationException($"{objName} can not be null or empty");
            }
        }
    }
}