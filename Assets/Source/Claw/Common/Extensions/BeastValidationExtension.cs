using System;

namespace Beast.Claw.Common
{
    public static class BeastValidationExtension
    {
        public static void ValidateThrowException<T>(this T data) where T : IValidatable
        {
            if (!data.IsValid(out string message))
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}