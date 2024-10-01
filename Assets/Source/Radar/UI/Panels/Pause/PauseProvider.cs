using System;
using Beast.Claw.Common;
using Beast.Radar.UI.Enums;
using Beast.Radar.UI.Panels.Base;

namespace Beast.Radar.UI.Panels.Pause
{
    public class PauseProvider : BProvider
    {
        public override bool ShowsBlackOverlay => true;
        public event Action LoadButtonPressed;
        public event Action NewGameButtonPressed;
        public event Action SaveButtonPressed;
        
        private readonly PauseView _view;

        
        public PauseProvider(PauseView view)
        {
            _view = view;
        }

        public void Initialize()
        {
            SubscribeToEvents(true);
        }

        public void Dispose()
        {
            SubscribeToEvents(false);
        }

        public override void OpenPanel(Action onCompleted = null)
        {
            _view.body.gameObject.SetActive(true);
            onCompleted?.Invoke();
        }

        public override void ClosePanel(Action onCompleted = null)
        {
            _view.body.gameObject.SetActive(false);
            onCompleted?.Invoke();
        }

        public void SetLoadButtonInteractable(bool interactable)
        {
            _view.loadButton.SetInteractable(interactable);
        }
        
        private void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _view.newGameButton.Pressed += OnNewGameButtonPressed;
                _view.loadButton.Pressed += OnLoadButtonPressed;
                _view.saveButton.Pressed += OnSaveButtonPressed;
                _view.closePanelButton.Pressed += OnCloseButtonPressed;
            }
            else
            {
                _view.newGameButton.Pressed -= OnNewGameButtonPressed;
                _view.loadButton.Pressed -= OnLoadButtonPressed;
                _view.saveButton.Pressed -= OnSaveButtonPressed;
                _view.closePanelButton.Pressed -= OnCloseButtonPressed;
            }
        }

        private void OnNewGameButtonPressed()
        {
            NewGameButtonPressed?.Invoke();
        }

        private void OnSaveButtonPressed()
        {
            SaveButtonPressed?.Invoke();
        }

        private void OnLoadButtonPressed()
        {
            LoadButtonPressed?.Invoke();
        }

        private void OnCloseButtonPressed()
        {
            InvokeOtherPanelCall(EPanelType.Hud);
        }
    }
}