using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;

namespace Beast.Radar.InputControl
{
    public interface IPlayerInputProvider : Initializable, Disposable
    {
        EInputScheme Scheme { get; }
    }
}