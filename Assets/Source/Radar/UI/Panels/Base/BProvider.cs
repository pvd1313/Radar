using System;
using Beast.Radar.UI.Enums;

namespace Beast.Radar.UI.Panels.Base
{
    public abstract class BProvider
    {
        public abstract bool ShowsBlackOverlay { get; }
        
        public event Action<EPanelType> OtherPanelCalled;
        public abstract void OpenPanel(Action onCompleted = null);
        public abstract void ClosePanel(Action onCompleted = null);

        protected void InvokeOtherPanelCall(EPanelType type)
        {
            OtherPanelCalled?.Invoke(type);
        }
    }
}