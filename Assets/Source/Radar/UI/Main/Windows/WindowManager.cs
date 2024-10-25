using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Radar.InputHandling;
using Cysharp.Threading.Tasks;
using ModestTree;

namespace Beast.Radar.UI
{
    public class WindowManager : IWindowManager, IGameCycleListener
    {
        public event Action<bool> WindowChanged;
        public event Action<bool> MouseOverUI;

        public string CurrentWindow => _currentWindowID ?? WindowID.None;
        
        private readonly IUIFactory _uiFactory;
        private readonly IInputSystem _inputSystem;
        private readonly Dictionary<string, IWindow> _allWindows = new();
        
        private CanvasView _canvas;
        private IWindow _currentWindow;
        private string _currentWindowID;
        
        public WindowManager(IUIFactory uiFactory, IInputSystem inputSystem)
        {
            _uiFactory = uiFactory;
            _inputSystem = inputSystem;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }
        
        private UniTask<OperationResult> _warmup(OperationContext context)
        {
            _canvas = _uiFactory.CreateCanvas(this);
                
            _subscribe();
            
            return OperationResult.SuccessSync();
        }

        public void Register(IWindow window, string windowID, UIView view)
        {
            if (_allWindows.ContainsKey(windowID))
            {
                throw new InvalidOperationException($"'{windowID}' is already registered");
            }

            int windowIndex = WindowID.WindowIndex.IndexOf(windowID);
            view.transform.SetParent(_canvas.transform, false);
            view.transform.SetSiblingIndex(windowIndex);
            
            _allWindows.Add(windowID, window);  
        }

        public void Show(string windowID)
        {
            if (!_allWindows.ContainsKey(windowID))
            {
                throw new InvalidOperationException($"Window with ID '{windowID}' is not registered or does not exist.");
            }

            _currentWindow?.Close();
            _currentWindow = _allWindows[windowID];
            _currentWindowID = windowID;
            _currentWindow.Open();
            
            bool isHasOpenedWindow = windowID != WindowID.MainHUD;
            
            WindowChanged?.Invoke(isHasOpenedWindow);
        }

        private void _subscribe()
        {
            _canvas.MouseOverUI += _onMouseOverUI;
            _inputSystem.UIInput.OptionButtons.PressStarted += _onOptionButtonsPressStarted;
        }

        private void _unsubscribe()
        {
            _canvas.MouseOverUI -= _onMouseOverUI;
            _inputSystem.UIInput.OptionButtons.PressStarted -= _onOptionButtonsPressStarted;
        }

        private void _onMouseOverUI(bool isOver)
        {
            MouseOverUI?.Invoke(isOver);       
        }

        private void _onOptionButtonsPressStarted(EInputOptionButton button)
        {
            if (button == EInputOptionButton.Fourth)
            {
                if (_currentWindowID != WindowID.MainHUD)
                {
                    Show(WindowID.MainHUD);
                }
            }
            else if(button == EInputOptionButton.Start)
            {
                Show(_currentWindow is not HUDWindow ? WindowID.MainHUD : WindowID.Pause);
            }
        }

        private UniTask<OperationResult> _dispose(OperationContext context)
        {
            _unsubscribe();

            return OperationResult.SuccessSync();
        }
    }
}