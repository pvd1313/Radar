using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class WindowManager : IWindowManager, Initializable
    {
        public event Action<bool> WindowChanged;
        public event Action<bool> MouseOverUI;

        private readonly IUIFactory _uiFactory;
        private readonly Dictionary<string, BeastUIWindowView> _allViews = new();

        private CanvasView _canvas;
        private BeastUIWindowView _currentOpenedView;

        public WindowManager(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public UniTask<InitializationResult> Initialize()
        {
            _canvas = _uiFactory.CreateCanvas(UIPrefabsID.Canvas);
            _canvas.MouseOverUI += over => MouseOverUI?.Invoke(over);
            return InitializationResult.SuccessNotAsync();
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
    }
}