using System;

namespace Beast.Radar.InputControl
{
    public interface IBeastPlayerInputProvider
    {
        EBeastInputScheme Scheme { get; }
        public InputActions Actions { get; }
        event Action<EBeastInputScheme> ChangedScheme;
    }
}