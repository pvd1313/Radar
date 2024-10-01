using System;
using Beast.Radar.UI.Enums;
using Beast.Radar.UI.Panels.Base;

namespace Beast.Radar.UI.Canvas
{
    public interface ICanvas
    {
        event Action<bool> MouseOverUI;
        public void AddChildProvider(EPanelType type, BProvider provider);
        public T CreateChildPanel<T>(bool isLastSabling) where T : BView;
    }
}