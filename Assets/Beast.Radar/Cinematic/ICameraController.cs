using Beast.Claw.GameCycle;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Cinematic
{
    public interface ICameraController : Initializable, Disposable, IFrameBeginUpdate, IFrameEndUpdate
    {
        public CameraControllerState State { get; }

    }
}