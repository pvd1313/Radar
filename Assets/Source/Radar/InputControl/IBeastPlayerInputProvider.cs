using System;
using Beast.Claw.GameCycle;

namespace Beast.Radar.InputControl
{
    public interface IBeastPlayerInputProvider : IBeastInitialize, IBeastDispose
    {
        EBeastInputScheme Scheme { get; }
        public InputActions Actions { get; }
        event Action<EBeastInputScheme> ChangedScheme;
    }
}