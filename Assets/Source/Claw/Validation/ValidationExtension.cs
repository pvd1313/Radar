using System;
using System.Runtime.CompilerServices;

namespace Beast.Claw
{
    public static class ValidationExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateThrowException<TData>(this TData data) where TData : IValidatable
        {
            string errorMessage = data.Validate();
            if (errorMessage != null)
            {
                throw new ValidationException(errorMessage);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid<TData>(this TData data, out string errorMessage) where TData : IValidatable
        {
            errorMessage = data.Validate();

            return errorMessage == null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryValidate<TData>(this TData data, out bool isValid, out string errorMessage) 
            where TData : class, IValidatable
        {
            if (data == null)
            {
                errorMessage = "Argument is null";
                isValid = false;
                return;
            }

            errorMessage = data.Validate();
            isValid = errorMessage == null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotNullThrowException<TArgument>(this TArgument obj, string objName)
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