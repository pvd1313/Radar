using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UISelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
   
        public event Action<ESelectionZone, int2> Selected;
        public event Action<ESelectionZone, int2> Deselected;

        [SerializeField] private Selectable _selectable;

        private ESelectionZone _zone;
        private int2 _index;
        private bool _isHighlighted;

        /*
        private void OnEnable()
        {
            if (IsSelected())
            {
                Select();
            }
        }
        */

        private void OnDisable()
        {
            Deselect();
        }

        public void SetIndex(int2 index)
        {
            _index = index;
        }

        public void SetZone(ESelectionZone zone)
        {
            _zone = zone;
        }

        public override string ToString()
        {
            return $"{_zone} {_index}";
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
            BDebug.LogInfo(this, $"{name} enter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Deselect();
            BDebug.LogInfo(this, $"{name} exit");
        }

        public void Select()
        {
            _selectable.Select();
            Selected?.Invoke(_zone, _index);
            BDebug.LogInfo(this, $"{name} select");
        }

        public void Deselect()
        {
            if (IsSelected())
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            BDebug.LogInfo(this, $"{name} deselect");
            Deselected?.Invoke(_zone, _index);
        }

        public void SetInteractable(bool isInteractable)
        {
            _selectable.interactable = isInteractable;
        }
        
        public bool IsSelected()
        {
            return EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        }
    }
}