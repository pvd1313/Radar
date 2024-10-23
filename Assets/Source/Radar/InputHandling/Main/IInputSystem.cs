using System;

namespace Beast.Radar.InputControl
{
    public interface IInputSystem
    {
        event Action<EInputScheme> ChangedScheme;
        public InputActions Actions { get; }
        EInputScheme Scheme { get; }
        public float ClickDuration { get; }
        public ICameraInputController GameActionMap { get; }
    }
}