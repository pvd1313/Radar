using System;

namespace Beast.Radar.UI
{
    public interface IWindowManager
    {
        event Action<bool> WindowChanged; 
        event Action<bool> MouseOverUI;
        public void RegisterPanel(string windowID, BeastUIWindowView view);
        public void ShowWindow(string windowID);
    }
}