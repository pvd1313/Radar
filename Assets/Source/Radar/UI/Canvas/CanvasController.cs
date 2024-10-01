using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.UI.Enums;
using Beast.Radar.UI.Factory;
using Beast.Radar.UI.Panels.Base;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI.Canvas
{
    public class CanvasController : BPanelController, ICanvas, Initializable
    {
        public event Action<bool> MouseOverUI;
        
        private readonly IUIFactory _uiFactory;
        private readonly Dictionary<EPanelType, BProvider> _childProviders = new();
        
        private CanvasProvider _canvasProvider;
        
        private BProvider _currentOpenedProvider;
        
        public CanvasController(IUIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        public UniTask<InitializationResult> Initialize()
        {
            CanvasView canvasView = _uiFactory.CreatePanel<CanvasView>();
            canvasView.MouseOverUI += over => MouseOverUI?.Invoke(over);
            _canvasProvider = new CanvasProvider(canvasView);
            
            return InitializationResult.SuccessNotAsync();
        }

        public void AddChildProvider(EPanelType type, BProvider provider)
        {
            if (_childProviders.ContainsKey(type))
            {
                return;
            }
            
            _childProviders.Add(type, provider);
            provider.OtherPanelCalled += OnOtherPanelCalled;
        }

        public T CreateChildPanel<T>(bool isLastSabling) where T : BView
        {
            T view = _uiFactory.CreatePanel<T>();
            _canvasProvider.AddChildPanel(view, isLastSabling);
            return view;
        }

        private void OnOtherPanelCalled(EPanelType type)
        {
            BProvider newProvider = _childProviders.ContainsKey(type)
                ? _childProviders[type]
                : null;
            
            if (newProvider == null || !newProvider.ShowsBlackOverlay)
            {
                 _canvasProvider.HideBlackOverlay();
            }
            else
            {
                 _canvasProvider.ShowBlackOverlay();
            }

            _currentOpenedProvider ??= _childProviders[EPanelType.Hud];
            _currentOpenedProvider?.ClosePanel(onCompleted: () =>
            {
                _currentOpenedProvider = newProvider;
                _currentOpenedProvider?.OpenPanel();
            });
        }
    }
}