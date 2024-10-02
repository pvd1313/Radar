using System;

namespace Beast.Radar.UI
{
    public interface IPanelDispatcher
    {
        event Action<bool> MouseOverUI;
        public T AddPanel<T>(string id, bool isLastSabling) where T : UIPanelView;
    }
}