using System;

namespace Beast.Radar.InputHandling
{
    public interface IInputSystem
    {
        public IInputSchemeObserver Scheme { get; }
        public IGameInputActions GameInput { get; }
        public IUIInputActions UIInput { get; }
        public IDebugInputActions DebugInput { get; }
    }
}