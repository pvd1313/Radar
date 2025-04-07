namespace Beast.Claw.Profiling
{
    public enum EBricksTreeBuildCode
    {
        Success = 1,
        Unknown = 2,
        StackIsNotEmptyAfterJobComplete = 3,
        StackOverflow = 4,
        StackUnderflow = 5,
        NameMismatch = 6,
    }
}