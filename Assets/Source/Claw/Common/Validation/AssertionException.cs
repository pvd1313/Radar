using System;

namespace Beast.Claw.Common
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
            
        }
    }
}