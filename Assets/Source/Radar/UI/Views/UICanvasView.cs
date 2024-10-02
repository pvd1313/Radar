using System;
using Beast.Claw.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Beast.Radar.UI
{
    public class UICanvasView : BeastMono, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<bool> MouseOverUI;
        public RectTransform rect;
        public string ID { get; set; }

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