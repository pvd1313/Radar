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
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private UniTask<OperationResult> Warmup()
        {
            _canvas = _uiFactory.CreateCanvas(this);
                
            SubscribeUI();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> Dispose()
        {
            UnsubscribeUI();

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

        private void SubscribeUI()
        {
            _canvas.MouseOverUI += OnMouseOverUI;
            _uiInputController.PauseButtonPressed += OnPauseButtonPressed;
        }

        private void UnsubscribeUI()
        {
            _canvas.MouseOverUI -= OnMouseOverUI;
            _uiInputController.PauseButtonPressed -= OnPauseButtonPressed;
        }

        private void OnMouseOverUI(bool isOver)
        {
            MouseOverUI?.Invoke(isOver);       
        }

        private void OnPauseButtonPressed()
        {
            Show(_currentOpenedWindowController is not PauseMenuWindow ? WindowID.Pause : WindowID.MainHUD);
        }
    }
}