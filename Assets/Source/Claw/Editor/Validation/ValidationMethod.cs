using System.Reflection;

namespace Beast.Claw.Editor.Validation
{
    internal struct ValidationMethod
    {
        public MethodInfo method;
        public object validator;
    }
}