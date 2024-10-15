using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableOutline : UIView,ISelectHandler ,IDeselectHandler
    {
        [SerializeField] private UISelectable _selectable;
        [SerializeField] private Outline _outline;
       
        private Coroutine _coroutine;

        private void OnEnable()
        {
            if (_selectable.IsSelected())
            {
                _outline.enabled = true;
            }
        }

        private void OnDisable()
        {
            _outline.enabled = false;
        }
        
        public void OnDeselect(BaseEventData eventData)
        {
            _outline.enabled = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            _outline.enabled = true;
        }
    }
}