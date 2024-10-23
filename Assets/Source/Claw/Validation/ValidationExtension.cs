using System.Runtime.CompilerServices;
using Unity.Mathematics;

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
        public static void ValidateIsZeroOrGreaterThrowException(this float x, string argumentName)
        {
            if (x < 0)
            {
                string message = $"'{argumentName}' must be zero or greater, but was: {x}";
                throw new ValidationException(message);
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
        public static void ValidateNotNullThrowException<TArgument>(this TArgument value, string valueName) 
            where TArgument : class
        {
            if (value == null)
            {
                throw new ValidationException($"'{valueName}' can not be null.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotEmptyOrNullThrowException(this string text, string argumentName)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ValidationException($"'{argumentName}' can not be null or empty.");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateComponentsBiggerThanZeroThrowException(this int2 vector, string argumentName)
        {
            if (vector.x <= 0 || vector.y <= 0)
            {
                string message = $"'{argumentName}' components must be bigger than zero, but was: {vector}.";
                throw new ValidationException(message);
            }
        }
    }
}