namespace Beast.Claw.Common
{
    public static class Build
    {
#if DEVELOPMENT
        public static readonly bool IS_DEVELOPMENT = true;
#else
        public static readonly bool IS_DEVELOPMENT = false;
#endif
            
            // TODO : https://risenbeast.youtrack.cloud/issue/R-62/Production-directive
#if !DEVELOPMENT
        public static readonly bool IS_PRODUCTION = true;
#else
        public static readonly bool IS_PRODUCTION = false;
#endif
            
#if (DEVELOPMENT && PRODUCTION) || (!DEVELOPMENT && !PRODUCTION)
        Wrong preprocessor directives. It should contain either DEVELOPMENT either PRODUCTION directive.
#endif
    }
}