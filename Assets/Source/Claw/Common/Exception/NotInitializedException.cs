using System;

namespace Beast.Claw.Common
{
    public class NotInitializedException : System.Exception
    {
        public NotInitializedException(string name) : base(name)
        {
            
        }

        public NotInitializedException(Type type) : base(type.FullName)
        {
            
        }
    }
}