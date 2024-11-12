namespace Beast.Claw.Common
{
    public static class Build
    {
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