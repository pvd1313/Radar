using System;

namespace Beast.Radar.UI
{
    public interface IWindowManager
    {
        event Action<bool> WindowChanged; 
        event Action<bool> MouseOverUI;

        void Register(IWindow window, string windowID, UIView view);
        void Show(string windowID);
    }
}