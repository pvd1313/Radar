using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.InputHandling;
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
        private IWindow _currentOpenedWindow;

        public WindowManager(IUIFactory uiFactory, IUIInputController input)
        {
            _uiFactory = uiFactory;
            _uiInputController = input;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(OperationID.WARMUP, _warmup);
            handle.ListenOperation(OperationID.DISPOSE, _dispose);
        }
        
        private UniTask<OperationResult> _warmup()
        {
            _canvas = _uiFactory.CreateCanvas(this);
                
            _subscribe();
            
            return OperationResult.SuccessSync();
        }

        private UniTask<OperationResult> _dispose()
        {
            _unsubscribe();

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

            _currentOpenedWindow?.Close();
            _currentOpenedWindow = _allWindows[windowID];
            _currentOpenedWindow.Open();
            
            bool isHasOpenedWindow = windowID != WindowID.MainHUD;
            
            WindowChanged?.Invoke(isHasOpenedWindow);
        }

        private void _subscribe()
        {
            _canvas.MouseOverUI += _onMouseOverUI;
            _uiInputController.PauseButtonPressed += _onPauseButtonPressed;
            _uiInputController.ActionButtonPressed += _onActionButtonPressed;
        }

        private void _unsubscribe()
        {
            _canvas.MouseOverUI -= _onMouseOverUI;
            _uiInputController.PauseButtonPressed -= _onPauseButtonPressed;
            _uiInputController.ActionButtonPressed -= _onActionButtonPressed;
        }

        private void _onMouseOverUI(bool isOver)
        {
            MouseOverUI?.Invoke(isOver);       
        }

        private void _onPauseButtonPressed()
        {
            Show(_currentOpenedWindow is not HUDWindow ? WindowID.MainHUD : WindowID.Pause);
        }

        private void _onActionButtonPressed(ESelectionZone zone)
        {
            if (zone == ESelectionZone.EastButton)
            {
                if (_currentOpenedWindow is not HUDWindow)
                {
                    Show(WindowID.MainHUD);
                }
            }
        }
    }
}