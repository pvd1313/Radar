using System;
using Beast.Claw.GameCycle;

namespace Beast.Radar.UI
{
    public interface IBeastWindowManager : Initializable, Disposable
    {
        event Action<bool> WindowChanged; 
        event Action<bool> MouseOverUI;
        
        void Register(IBeastUIWindow window, string windowID, BeastUIWindowView view);
        void Show(string windowID);
    }
}