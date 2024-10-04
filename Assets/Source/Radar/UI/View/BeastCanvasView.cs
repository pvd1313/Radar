using System;
using UnityEngine.EventSystems;

namespace Beast.Radar.UI
{
    public class BeastCanvasView : BeastUIView, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<bool> MouseOverUI;
        
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