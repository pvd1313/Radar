using System.Reflection;

namespace Beast.Claw.DataValidation
{
    internal struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}