using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw
{
    public static class ValidationExtension
    {
        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateThrowException<TData>(this TData data, string argumentName) where TData : IValidatable
        {
            string errorMessage = data.Validate();
            if (errorMessage != null)
            {
                throw new ValidationException(argumentName + ": " + errorMessage);
            }
        }
        
        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateIsZeroOrGreaterThrowException(this float x, string argumentName)
        {
            if (x < 0)
            {
                string message = $"'{argumentName}' must be zero or greater, but was: {x}";
                throw new ValidationException(message);
            }
        }
        
        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotNullThrowException<TArgument>(this TArgument value, string valueName) 
            where TArgument : class
        {
            if (value == null)
            {
                throw new ValidationException($"'{valueName}' can not be null.");
            }
        }

        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateArrayIndex(this int arrayIndex, int maxItemCount)
        {
            if (arrayIndex < 0 || arrayIndex >= maxItemCount)
            {
                throw new IndexOutOfRangeException();
            }
        }

        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateNotEmptyOrNullThrowException(this string text, string argumentName)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ValidationException($"'{argumentName}' can not be null or empty.");
            }
        }

        [Conditional("DEVELOPMENT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ValidateIsCreated<T>(this NativeArray<T> array) where T : unmanaged
        {
            if (!array.IsCreated)
            {
                throw new ValidationException("Array is disposed or was not created");
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
    }
}