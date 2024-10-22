using System;

namespace Beast.Radar.InputControl
{
    public interface IInputActionsProvider
    {
        event Action<EInputScheme> ChangedScheme;
        EInputScheme Scheme { get; }
        public InputActions Actions { get; }
    }
}