using System;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI
{
    public abstract class UIPanelView : BeastMono
    {
        public event Action<string> OtherPanelCalled;
        
        public RectTransform rect;
        public bool isShowsBlackOverlay;
        
        public virtual void OpenPanel(Action onCompleted = null)
        {
            rect.gameObject.SetActive(true);
            onCompleted?.Invoke();
        }

        public virtual void ClosePanel(Action onCompleted = null)
        {
            rect.gameObject.SetActive(false);
            onCompleted?.Invoke();
        }

        public void InvokeOtherPanelCall(string id)
        {
            OtherPanelCalled?.Invoke(id);
        }
    }
}