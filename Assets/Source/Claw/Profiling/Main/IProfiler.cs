namespace Beast.Claw.Profiling
{
    public interface IProfiler
    {
        ProfilingTreeReader Reader { get; }
        
        Profiler.Handle GetHandle(object owner);
    }
}