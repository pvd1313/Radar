using System.Reflection;
using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Reflection
{
    [ConstantGroup]
    public static class Binding
    {
        public const BindingFlags PUBLIC = BindingFlags.Public;
        public const BindingFlags PRIVATE = BindingFlags.NonPublic;
        public const BindingFlags INSTANCE = BindingFlags.Instance;
        
        public const BindingFlags INSTANCE_PUBLIC_PRIVATE = INSTANCE | PUBLIC | PRIVATE;
    }
}