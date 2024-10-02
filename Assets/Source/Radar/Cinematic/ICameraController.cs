using Beast.Claw.GameCycle;

namespace Beast.Radar.Cinematic
{
    public interface ICameraController : Initializable, IDisposable
    {
        public CameraControllerState State { get; }

    }
}