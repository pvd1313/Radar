using System;
using Beast.Claw.GameCycle;

namespace Beast.Radar.InputControl
{
    public interface IUIInputController : Initializable, Disposable
    {
        public event Action PausePressed;
    }
}