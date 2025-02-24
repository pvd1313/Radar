using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Common
{ 
    [ConstantGroup]
    public static class Build
    {
        public const int SMALL_ARRAY_SAFE_CAPACITY = 64;

        public const int ECS_PARALLEL_JOB_INNER_LOOP_BATCH_COUNT = 4;
            
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

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        public static readonly bool IS_MOBILE_PLATFORM = true;
#else
        public static readonly bool IS_MOBILE_PLATFORM = false;
#endif
    }
}