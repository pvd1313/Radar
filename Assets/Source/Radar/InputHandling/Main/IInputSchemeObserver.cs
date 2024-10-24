using System;

namespace Beast.Radar.InputHandling
{
    public interface IInputSchemeObserver
    {
        public event Action<EInputScheme> Changed;
        public EInputScheme Current { get; }
        public bool IsHardwareInput { get; }

    }
}