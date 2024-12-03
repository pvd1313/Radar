using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphy;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI
{
    public class GraphyWindowNoesis : Actor, IGraphyWindow, IReady
    {
        public bool IsReady { get; private set; }

        [Required] private readonly PlayerPreference _preference;
        [Required] private readonly UIConfig _uiConfig;
        [Required] private readonly IGraphyManager _manager;
        [Required] private readonly GraphyConfig _config;
        [Required] private readonly IWindowManager _windowManager;

        private GraphyView _view;
        private FpsPopup _fpsPopup;
        private InformationPopup _informationPopup;


        public void Open()
        {
            if (_view == null)
            {
                _uiConfig.graphyAsset.Instantiate(out _view);
                _view.InitializeFpsBrushes(_config.goodFpsColor, _config.cautionFpsColor, _config.criticalFpsColor);

                _windowManager.Attach(_view, 100);

                _fpsPopup = new FpsPopup(_preference, _config, _manager.FpsMonitor, _view.FpsAssets);
                _view.FpsAssets.SetFpsTexture(_manager.FpsRender.TextureGraph._material);
                _fpsPopup.Initialize();
                _fpsPopup.SetState();

                _informationPopup = new InformationPopup(_preference, _view.InformationAssets, _manager.InformationMonitor);
                _informationPopup.Initialize();

                IsReady = true;
            }
            else
            {
                _view.Visibility = Visibility.Visible;
            }
        }

        protected override void _tick(BlockingContext context)
        {
            if (IsReady)
            {
                _fpsPopup.UpdateText();
            }
        }

        public void Close()
        {
            if (_view != null)
            {
                _view.Visibility = Visibility.Collapsed;
            }
        }

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            if (_preference.isShowGraphy)
            {
                Open();
            }

            return UniTask.CompletedTask;
        }
    }
}