using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UISelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
    {
        public event Action<ESelectionZone, int2> Selected;
        
        [SerializeField] private Selectable _selectable;
        
        private ESelectionZone _zone;
        private int2 _index;

        public void SetIndex(int2 index)
        {
            _index = index;
        }

        public void SetZone(ESelectionZone zone)
        {
            _zone = zone;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _selectable.Select();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _selectable.OnDeselect(null);
        }

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke(_zone, _index);
        }

        public void Select()
        {
            _selectable.Select();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _selectable.OnDeselect(eventData);
        }

        public override string ToString()
        {
            return $"{_zone} {_index}";
        }
    }
}