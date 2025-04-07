namespace Beast.Claw.Profiling
{
    partial class BricksProfiler
    {
        public readonly struct Handle
        {
            public static Handle Disabled { get; } = default;
            
            private readonly BricksProfiler _manager;
            private readonly bool _isEnabled;

            public Handle(BricksProfiler manager, bool isEnabled)
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