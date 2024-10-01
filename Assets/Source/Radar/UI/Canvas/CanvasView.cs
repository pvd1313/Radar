using System;
using Beast.Radar.UI.Panels.Base;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beast.Radar.UI.Canvas
{
    public class CanvasView : BView, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<bool> MouseOverUI;
        [Space]
        public CanvasGroup blackOverlay;
        
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseOverUI?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            MouseOverUI?.Invoke(false);
        }
    }
}