using Beast.Claw.GameCycle;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public interface ICameraController : Initializable, Disposable
    {
        public CameraControllerState State { get; }

    }
}