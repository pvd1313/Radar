using System;
using Beast.Claw.GameCycle;

namespace Beast.Radar.InputControl
{
    public interface IPlayerInputProvider : Initializable, Disposable
    {
        EInputScheme Scheme { get; }
        event Action<EInputScheme> ChangedScheme;
    }
}