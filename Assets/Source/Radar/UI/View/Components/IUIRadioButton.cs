using System;

namespace Beast.Radar.UI
{
    public interface IUIRadioButton<T>
    {
        event Action<T> Clicked; 
        void SetSelectedMode();
        void SetDefaultMode();
    }
}