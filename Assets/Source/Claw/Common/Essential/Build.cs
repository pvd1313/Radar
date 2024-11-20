using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Common
{ 
    [ConstantGroup]
    public static class Build
    {
        public const int SMALL_ARRAY_SAFE_CAPACITY = 64;
            
#if DEVELOPMENT
        public static readonly bool IS_DEVELOPMENT = true;
#else
        public const bool IS_DEVELOPMENT = false;
#endif

#if UNITY_EDITOR
        public static readonly bool IS_EDITOR = true;
#else
        public const bool IS_EDITOR = false;
#endif
    }
}