using Beast.Claw.GameCycle;

namespace Beast.Radar.Cinematic
{
    public interface IPlayerInputProvider : Initializable, Disposable
    {
        EInputScheme InputScheme { get; }
    }
}