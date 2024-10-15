using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableOutline : UIView
    {
        [SerializeField] private UISelectable _selectable;
        [SerializeField] private Outline _outline;
       
        private Coroutine _coroutine;
        
        private void Start()
        {
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void OnDisable()
        {
            _outline.enabled = false;
        }
        
        private void OnEnable()
        {
            if (_selectable != null && _selectable.IsSelected)
            {
                if (_coroutine != null)
                {
                    StopCoroutine(_coroutine);
                }

                StartCoroutine(EnableRoutine());
            }
        }
        
        private IEnumerator EnableRoutine()
        {
            yield return new WaitForEndOfFrame();
            if (_selectable.IsSelected)
            {
                _outline.enabled = true;
            }
        }

        private void Subscribe()
        {
            _selectable.Selected += OnSelected;
            _selectable.Deselected += OnDeselected;
        }

        private void Unsubscribe()
        {
            _selectable.Selected -= OnSelected;
            _selectable.Deselected -= OnDeselected;
        }

        private void OnSelected(ESelectionZone arg1, int2 arg2)
        {
            _outline.enabled = true;
        }

        private void OnDeselected(ESelectionZone arg1, int2 arg2)
        {
            _outline.enabled = false;
        }
    }
}