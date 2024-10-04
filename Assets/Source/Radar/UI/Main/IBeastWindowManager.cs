using System;

namespace Beast.Radar.UI
{
    public interface IBeastWindowManager
    {
        event Action<bool> WindowChanged; 
        event Action<bool> MouseOverUI;
        
        void Register(IBeastUIWindow window, string windowID, BeastUIView view);
        void Show(string windowID);
    }
}