using Beast.Claw.GameCycle;

namespace Beast.Radar.Cinematic
{
    public interface ICameraController : IBeastInitialize, IBeastDispose
    {
        public CameraControllerState State { get; }

    }
}