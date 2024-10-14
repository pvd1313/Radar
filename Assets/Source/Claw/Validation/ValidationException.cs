using System;

namespace Beast.Claw
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base (message)
        {
            
        }
    }
}