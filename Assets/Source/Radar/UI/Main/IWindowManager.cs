using System;
using Beast.Claw.GameCycle;

namespace Beast.Radar.UI
{
    public interface IWindowManager : Initializable, Disposable
    {
        event Action<bool> WindowChanged; 
        event Action<bool> MouseOverUI;
        public void RegisterWindow(string windowID, BeastUIWindowView view, BeastUIWindow window);
        public void ShowWindow(string windowID);
    }
}