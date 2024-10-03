using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class WindowManager : IWindowManager
    {
        public event Action<bool> WindowChanged;
        public event Action<bool> MouseOverUI;

        private readonly IUIFactory _uiFactory;
        private readonly IUIInputController _uiInputController;
        private readonly Dictionary<string, BeastUIWindowView> _allViews = new();

        private CanvasView _canvas;
        private BeastUIWindowView _currentOpenedView;

        public WindowManager(IUIFactory uiFactory, IUIInputController uiInputController)
        {
            _uiFactory = uiFactory;
            _uiInputController = uiInputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _canvas = _uiFactory.CreateCanvas(UIPrefabsID.Canvas);
            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }
        
        public void RegisterPanel(string windowID, BeastUIWindowView view)
        {
            if (_allViews.ContainsKey(windowID))
            {
                throw new BeastException($"Window with id #{windowID} already exists");
            }
            
            view.transform.SetParent(_canvas.transform, false);
            _allViews.Add(windowID, view);
        }

        public void ShowWindow(string windowID)
        {
            if (!_allViews.ContainsKey(windowID))
            {
                throw new BeastException($"Window with id #{windowID} already exists");
            }
            
            if (_currentOpenedView != null && _currentOpenedView as MainHUDView == null)
            {
                _currentOpenedView.gameObject.SetActive(false);
            }

            _currentOpenedView = _allViews[windowID];
            bool isHasOpenedWindow = false;

            if (_currentOpenedView as MainHUDView == null)
            {
                _currentOpenedView.gameObject.SetActive(true);
                isHasOpenedWindow = true;
            }
            
            WindowChanged?.Invoke(isHasOpenedWindow);
        }

        private void Subscribe()
        {
            _canvas.MouseOverUI += OnMouseOverUI;
            _uiInputController.PausePressed += OnPausePressed;
        }

        private void Unsubscribe()
        {
            _canvas.MouseOverUI -= OnMouseOverUI;
            _uiInputController.PausePressed -= OnPausePressed;
        }

        private void OnMouseOverUI(bool isOver)
        {
            MouseOverUI?.Invoke(isOver);       
        }

        private void OnPausePressed()
        {
            ShowWindow(_currentOpenedView as PauseView == null ? UIWindowID.Pause : UIWindowID.MainHUD);
        }
    }
}