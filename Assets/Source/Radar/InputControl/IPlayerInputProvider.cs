using System;
using Beast.Claw.GameCycle;
using IDisposable = Beast.Claw.GameCycle.IDisposable;

namespace Beast.Radar.InputControl
{
    public interface IPlayerInputProvider : Initializable, IDisposable
    {
        EInputScheme Scheme { get; }
        event Action<EInputScheme> ChangedScheme;
    }
}