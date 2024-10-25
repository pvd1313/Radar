namespace Beast.Claw.Profiling
{
    public interface IProfiler
    {
        ProfilerTreeReader Reader { get; }
        
        Profiler.Handle GetHandle(object owner);
    }
}