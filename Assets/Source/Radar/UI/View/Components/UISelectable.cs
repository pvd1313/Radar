using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class UISelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler , ISelectHandler, IDeselectHandler
    {
        public event Action<ESelectionZone, int2> Selected;

        [SerializeField] private Selectable _selectable;
        [SerializeField, CanBeNull] private SelectableVisual _selectableVisual;
        
        private ESelectionZone _zone;
        private int2 _index;
        private bool _isHighlighted;
        
        public void SetIndex(int2 index)
        {
            _index = index;
        }

        public void SetZone(ESelectionZone zone)
        {
            _zone = zone;
        }

        public void Select()
        {
            _selectable.Select();
            Selected?.Invoke(_zone, _index);
        }

        public void Deselect()
        {
            if (IsSelected())
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            _selectable.interactable = isInteractable;
        }

        public bool IsInteractable()
        {
            return _selectable.interactable;
        }

        public bool IsSelected()
        {
            return EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        }

        public override string ToString()
        {
            return $"{_zone} {_index}";
        }
        
        #region Unity events
        
        private void OnEnable()
        {
            if (IsSelected() && _selectableVisual != null)
            {
                _selectableVisual.SetSelectedMode();
            }
        }
        
        private void OnDisable()
        {
            Deselect();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_selectableVisual != null && IsInteractable())
            {
                _selectableVisual.SetSelectedMode();
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_selectableVisual != null)
            {
                _selectableVisual.SetDefaultMode();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Select();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Deselect();
        }

        #endregion
    }
}