using System;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIStepButtons : UIView
    {
        public UIButton Increment => _incrementButton;
        public UIButton Decrement => _decrementButton;
        
        public event Action<int> Clicked;

        [SerializeField] private UIButton _incrementButton;
        [SerializeField] private UIButton _decrementButton;

        private void Start()
        {
            _subscribe();
        }

        private void OnDestroy()
        {
            _unsubscribe();
        }

        public void SetInteractableIncrementButton(bool interactable)
        {
            _incrementButton.SetInteractable(interactable);
        }

        private void SetInteractableDecrementButton(bool interactable)
        {
            _decrementButton.SetInteractable(interactable);
        }
        
        private void _subscribe()
        {
            _incrementButton.Clicked += _onClickedIncrementButton;
            _decrementButton.Clicked += _onClickedDecrementButton;
        }

        private void _unsubscribe()
        {
            _incrementButton.Clicked -= _onClickedIncrementButton;
            _decrementButton.Clicked -= _onClickedDecrementButton;
        }

        private void _onClickedIncrementButton()
        {
            Clicked?.Invoke(1);
        }

        private void _onClickedDecrementButton()
        {
            Clicked?.Invoke(-1);
        }
    }
}