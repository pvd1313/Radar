using System;

namespace Beast.Claw.Validation
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base (message)
        {
            
        }
    }
}