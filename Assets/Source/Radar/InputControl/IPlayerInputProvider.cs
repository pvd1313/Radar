using System;

namespace Beast.Radar.InputControl
{
    public interface IPlayerInputProvider
    {
        EInputScheme Scheme { get; }
        public InputActions Actions { get; }
        event Action<EInputScheme> ChangedScheme;
    }
}