using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public class PanelDispatcher : IPanelDispatcher, Initializable
    {
        public event Action<bool> MouseOverUI;

        private readonly IUIFactory _uiFactory;
        private readonly Dictionary<string, UIPanelView> _allViews = new();

        private UICanvasView _uiCanvas;
        private UIOverlayView _uiOverlay;
        private UIPanelView _currentOpenedView;

        public PanelDispatcher(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public UniTask<InitializationResult> Initialize()
        {
            UICanvasView mainUICanvasView = _uiFactory.CreateCanvas(UIComponentsIDs.CanvasMain);
            mainUICanvasView.MouseOverUI += over => MouseOverUI?.Invoke(over);
            _uiOverlay = _uiFactory.CreateOverlay(UIComponentsIDs.OverlayBlack, UIComponentsIDs.CanvasMain, false);
            return InitializationResult.SuccessNotAsync();
        }

        public T AddPanel<T>(string id, bool isLastSabling) where T : UIPanelView
        {
            if (_allViews.ContainsKey(id))
            {
                throw new BeastException($"Panel with id #{id} already exists");
            }
            UIPanelView view = _uiFactory.CreatePanel(id, UIComponentsIDs.CanvasMain, isLastSabling);
            view.OtherPanelCalled += OnOtherPanelCalled;
            _allViews.Add(id, view);
            return view as T;
        }
        
        private void OnOtherPanelCalled(string panelID)
        {
            UIPanelView otherView = _allViews.ContainsKey(panelID)
                ? _allViews[panelID]
                : null;
            
            RefreshBlackOverlay(otherView);
            SwitchView(otherView);
        }

        private void SwitchView(UIPanelView otherView)
        {
            if (_currentOpenedView is MainHUDView or null)
            {
                Open(otherView);
            }
            else
            {
                _currentOpenedView.ClosePanel(onCompleted: () =>
                {
                    Open(otherView);
                });
            }

            void Open(UIPanelView view)
            {
                _currentOpenedView = view;
                if (_currentOpenedView as MainHUDView == null)
                {
                    _currentOpenedView.OpenPanel();
                }
            }
        }

        private void RefreshBlackOverlay(UIPanelView otherView)
        {
            if (otherView == null || !otherView.isShowsBlackOverlay)
            {
                _uiOverlay.Hide();
            }
            else
            {
                _uiOverlay.Show();
            }
        }
    }
}