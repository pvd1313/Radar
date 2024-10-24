namespace Beast.Claw.Profiling
{
    partial class Profiler
    {
        public readonly struct Handle
        {
            public static Handle Disabled { get; } = new (null, false);
            
            private readonly Profiler _manager;
            private readonly bool _isEnabled;

            public Handle(Profiler manager, bool isEnabled)
            {
                _manager = manager;
                _isEnabled = isEnabled;
            }

            public void BeginSample(string name)
            {
                if (!_isEnabled)
                {
                    return;
                }
                
                _manager._beginSample(name);
            }

            public void EndSample(string name)
            {
                if (!_isEnabled)
                {
                    return;
                }
                
                _manager._endSample(name);
            }
        }
    }
}