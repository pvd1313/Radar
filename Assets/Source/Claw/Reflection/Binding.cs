using System.Reflection;

namespace Beast.Claw.Reflection
{
    public static class Binding
    {
        public const BindingFlags PUBLIC = BindingFlags.Public;
        public const BindingFlags PRIVATE = BindingFlags.NonPublic;
        public const BindingFlags INSTANCE = BindingFlags.Instance;
        
        public const BindingFlags INSTANCE_PUBLIC_PRIVATE = INSTANCE | PUBLIC | PRIVATE;
    }
}