using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.InputControl;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class BeastWindowManager : IBeastWindowManager, IBeastGameCycleListener
    {
        public event Action<bool> WindowChanged;
        public event Action<bool> MouseOverUI;

        private readonly IBeastUIFactory _uiFactory;
        private readonly IBeastUIInputController _uiInputController;
        private readonly Dictionary<string, IBeastUIWindow> _allWindows = new();

        private BeastCanvasView _canvas;
        private IBeastUIWindow _currentOpenedWindowController;

        public BeastWindowManager(IBeastUIFactory uiFactory, IBeastUIInputController uiInputController)
        {
            _uiFactory = uiFactory;
            _uiInputController = uiInputController;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Warmup, Warmup);
            handle.ListenOperation(this, BeastCycleEventID.Dispose, Dispose);
        }
        
        private UniTask<BeastInitializationResult> Warmup()
        {
            _canvas = _uiFactory.CreateCanvas(this);
                
            SubscribeUI();
            
            return BeastInitializationResult.SuccessSync();
        }

        private UniTask<BeastInitializationResult> Dispose()
        {
            UnsubscribeUI();

            return BeastInitializationResult.SuccessSync();
        }
        
        public void Register(IBeastUIWindow window, string windowID, BeastUIView view)
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
            _currentOpenedWindowController = _allWindows[windowID];
            _currentOpenedWindowController.Open();
            
            bool isHasOpenedWindow = windowID != BeastUIWindowID.MainHUD;
            
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
            Show(_currentOpenedWindowController is not BeastPauseWindow ? BeastUIWindowID.Pause : BeastUIWindowID.MainHUD);
        }
    }
}