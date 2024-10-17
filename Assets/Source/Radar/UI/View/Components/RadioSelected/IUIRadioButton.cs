using System;

namespace Beast.Radar.UI
{
    public interface IUIRadioButton<TButton> : IRadioSelected
    {
        event Action<TButton> Clicked;
    }
}