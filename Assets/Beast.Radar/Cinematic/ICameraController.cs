using Beast.Claw.GameCycle;

namespace Beast.Radar.Cinematic
{
    public interface ICameraController : Initializable, Disposable
    {
        public CameraControllerState State { get; }

    }
}