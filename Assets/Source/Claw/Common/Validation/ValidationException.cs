using System;

namespace Beast.Claw.Common
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base (message)
        {
            
        }
    }
}