using System;
using Beast.Claw.GameCycle;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IUIInputController : Initializable, Disposable, IFrameBeginUpdate
    {
        public event Action PausePressed;
        public event Action<float2> NavigationPressed;
    }
}