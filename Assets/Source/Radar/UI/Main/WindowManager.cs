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
        private readonly Dictionary<string, BeastUIWindow> _allWindows = new();

        private CanvasView _canvas;
        private BeastUIWindow _currentOpenedWindow;

        public WindowManager(IUIFactory uiFactory, IUIInputController uiInputController)
        {
            _uiFactory = uiFactory;
            _uiInputController = uiInputController;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _canvas = _uiFactory.CreateCanvas(UIPrefabID.Canvas);
            Subscribe();
            return InitializationResult.SuccessNotAsync();
        }

        public void Dispose()
        {
            Unsubscribe();
        }
        
        public void RegisterWindow(string windowID, BeastUIWindowView view, BeastUIWindow window)
        {
            if (_allWindows.ContainsKey(windowID))
            {
                throw new BeastException($"Window with id #{windowID} already exists");
            }
            
            view.transform.SetParent(_canvas.transform, false);
            _allWindows.Add(windowID, window);
        }

        public void ShowWindow(string windowID)
        {
            if (!_allWindows.ContainsKey(windowID))
            {
                throw new BeastException($"Window with id #{windowID} already exists");
            }

            _currentOpenedWindow?.Close();
            _currentOpenedWindow = _allWindows[windowID];
            _currentOpenedWindow.Open();
            
            bool isHasOpenedWindow = windowID != UIWindowID.MainHUD;
            
            WindowChanged?.Invoke(isHasOpenedWindow);
        }

        private void Subscribe()
        {
            _canvas.MouseOverUI += OnMouseOverUI;
            _uiInputController.PausePressed += OnPauseKeyPressed;
        }

        private void Unsubscribe()
        {
            _canvas.MouseOverUI -= OnMouseOverUI;
            _uiInputController.PausePressed -= OnPauseKeyPressed;
        }

        private void OnMouseOverUI(bool isOver)
        {
            MouseOverUI?.Invoke(isOver);       
        }

        private void OnPauseKeyPressed()
        {
            ShowWindow(_currentOpenedWindow is not PauseWindow ? UIWindowID.Pause : UIWindowID.MainHUD);
        }
    }
}