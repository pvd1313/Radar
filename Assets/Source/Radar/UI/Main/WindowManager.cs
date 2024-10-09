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
        private readonly ISelectableManager _selectableManager;
        private readonly Dictionary<string, IWindow> _allWindows = new();

        private CanvasView _canvas;
        private IWindow _currentOpenedWindowController;

        public WindowManager(IUIFactory uiFactory, IUIInputController input, ISelectableManager selectableManager)
        {
            _uiFactory = uiFactory;
            _uiInputController = input;
            _selectableManager = selectableManager;
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
                throw new InvalidOperationException($"'{windowID}' is already shown");
            }

            _currentOpenedWindowController?.Close();
            _selectableManager.Reset();
            _currentOpenedWindowController = _allWindows[windowID];
            _currentOpenedWindowController.Open();
            _selectableManager.Set(_currentOpenedWindowController.Selections);
            
            bool isHasOpenedWindow = windowID != WindowID.MainHUD;
            
            WindowChanged?.Invoke(isHasOpenedWindow);
        }

        private void SubscribeUI()
        {
            _canvas.MouseOverUI += OnMouseOverUI;
            _uiInputController.PausePressed += OnPauseKeyPressed;
        }

        private void UnsubscribeUI()
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
            Show(_currentOpenedWindowController is not PauseMenuWindow ? WindowID.Pause : WindowID.MainHUD);
        }
    }
}