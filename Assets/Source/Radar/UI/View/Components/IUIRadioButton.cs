using System;

namespace Beast.Radar.UI
{
    public interface IUIRadioButton<T> : IRadioSelected
    {
        event Action<T> Clicked;
    }
}