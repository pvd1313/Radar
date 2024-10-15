using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class WindowManager : IWindowManager, IGameCycleListener
    {
        public event Action<bool> WindowChanged;
        public event Action<bool> MouseOverUI;

        private readonly IUIFactory _uiFactory;
        private readonly IUIInputController _uiInputController;
        private readonly Dictionary<string, IWindow> _allWindows = new();

        private CanvasView _canvas;
        private IWindow _currentOpenedWindowController;

        public WindowManager(IUIFactory uiFactory, IUIInputController input)
        {
            _uiFactory = uiFactory;
            _uiInputController = input;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
        }
        
        private UniTask<OperationResult> _warmup()
        {
            _canvas = _uiFactory.CreateCanvas(this);
                
            _subscribeUI();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribeUI();

            return OperationResult.SuccessSync();
        }
        
        public void Register(IWindow window, string windowID, UIView view)
        {
            if (_allWindows.ContainsKey(windowID))
            {
                throw new InvalidOperationException($"'{windowID}' is already registered");
            }
            
            view.transform.SetParent(_canvas.transform, false);
            _allWindows.Add(windowID, window);  
        }

        public void Show(string windowID)
        {
            if (!_allWindows.ContainsKey(windowID))
            {
                throw new InvalidOperationException($"Window with ID '{windowID}' is not registered or does not exist.");
            }

            _currentOpenedWindowController?.Close();
            _currentOpenedWindowController = _allWindows[windowID];
            _currentOpenedWindowController.Open();
            
            bool isHasOpenedWindow = windowID != WindowID.MainHUD;
            
            WindowChanged?.Invoke(isHasOpenedWindow);
        }

        private void _subscribeUI()
        {
            _canvas.MouseOverUI += _onMouseOverUI;
            _uiInputController.PauseButtonPressed += _onPauseButtonPressed;
        }

        private void _unsubscribeUI()
        {
            _canvas.MouseOverUI -= _onMouseOverUI;
            _uiInputController.PauseButtonPressed -= _onPauseButtonPressed;
        }

        private void _onMouseOverUI(bool isOver)
        {
            MouseOverUI?.Invoke(isOver);       
        }

        private void _onPauseButtonPressed()
        {
            Show(_currentOpenedWindowController is not HUDWindow ? WindowID.MainHUD : WindowID.Pause);
        }
    }
}