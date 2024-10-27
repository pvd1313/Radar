using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using ModestTree;

namespace Beast.Radar.UI
{
    public class WindowManager : IWindowManager, IGameCycleListener
    {
        public event Action<string> WindowChanged;
        public event Action<bool> MouseOverUI;

        public string CurrentWindow => _currentWindowID ?? WindowID.None;
        
        private readonly IUIFactory _uiFactory;
        // private readonly IInputSystem _inputSystem;
        
        private Dictionary<string, IWindow> _windows;
        private CanvasView _canvas;
        private IWindow _currentWindow;
        private string _currentWindowID;
        
        public WindowManager(IUIFactory uiFactory/*, IInputSystem inputSystem*/)
        {
            _uiFactory = uiFactory;
            // _inputSystem = inputSystem;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
        }
        
        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _windows = new Dictionary<string, IWindow>();
            
            _canvas = _uiFactory.CreateCanvas(this);
                
            _subscribe();
            
            return ActionResult.SuccessSyncronous();
        }

        public void Register(IWindow window, string windowID, UIView view)
        {
            if (_windows.ContainsKey(windowID))
            {
                throw new InvalidOperationException($"'{windowID}' is already registered");
            }

            int windowIndex = WindowID.WindowIndex.IndexOf(windowID);
            view.transform.SetParent(_canvas.transform, false);
            view.transform.SetSiblingIndex(windowIndex);
            
            _windows.Add(windowID, window);  
        }

        public void Show(string windowID)
        {
            if (!_windows.TryGetValue(windowID, out IWindow newWindow))
            {
                throw new InvalidOperationException($"Window '{windowID}' was not found.");
            }
            
            _currentWindow?.Close();
            _currentWindow = newWindow;
            _currentWindowID = windowID;
            _currentWindow.Open();

            if (_currentWindow != newWindow)
            {
                WindowChanged?.Invoke(windowID);
            }
        }

        private void _subscribe()
        {
            _canvas.MouseOverUI += _onMouseOverUI;
            // _inputSystem.UIInput.OptionButtons.PressStarted += _onOptionButtonsPressStarted;
        }

        private void _unsubscribe()
        {
            _canvas.MouseOverUI -= _onMouseOverUI;
            // _inputSystem.UIInput.OptionButtons.PressStarted -= _onOptionButtonsPressStarted;
        }

        private void _onMouseOverUI(bool isMouseOverUI)
        {
            MouseOverUI?.Invoke(isMouseOverUI);       
        }

        // private void _onOptionButtonsPressStarted(EInputOptionButton button)
        // {
        //     if (button == EInputOptionButton.Fourth)
        //     {
        //         if (_currentWindowID != WindowID.MainHUD)
        //         {
        //             Show(WindowID.MainHUD);
        //         }
        //     }
        //     else if(button == EInputOptionButton.Start)
        //     {
        //         Show(_currentWindow is not HUDWindow ? WindowID.MainHUD : WindowID.Pause);
        //     }
        // }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            _unsubscribe();

            return ActionResult.SuccessSyncronous();
        }
    }
}