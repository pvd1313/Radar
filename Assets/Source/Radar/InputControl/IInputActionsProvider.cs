using System;

namespace Beast.Radar.InputControl
{
    public interface IInputActionsProvider
    {
        EInputScheme Scheme { get; }
        public global::InputActions Actions { get; }
        event Action<EInputScheme> ChangedScheme;
    }
}