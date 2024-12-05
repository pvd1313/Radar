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
        private AudioPopup _audioPopup;
        private RamPopup _ramPopup;

        protected override UniTask _warmup(ConcurrentContext arg)
        {
            if (_preference.graphy.isShowGraphy)
            {
                Open();
            }

            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            if (IsReady && _preference.graphy.isShowGraphy)
            {
                _fpsPopup.Update();
                _audioPopup.Update();
                _ramPopup.Update();
            }
        }

        public void Open()
        {
            if (_view == null)
            {
                _initialize();
            }
            else
            {
                _view.Visibility = Visibility.Visible;
            }
        }

        public void Close()
        {
            if (_view != null)
            {
                _view.Visibility = Visibility.Collapsed;
            }
        }

        private void _initialize()
        {
            _uiConfig.graphyAsset.Instantiate(out _view);
            _windowManager.Attach(_view, 100);

            _view.InitializeFpsBrushes(_config.goodFpsColor, _config.cautionFpsColor, _config.criticalFpsColor);

            _fpsPopup = new FpsPopup(_preference, _config, _manager.FpsMonitor, _manager.FpsRender, _view.FpsAssets);
            _fpsPopup.Initialize();
            _fpsPopup.RefreshState();

            _informationPopup = new InformationPopup(_preference, _view.InformationAssets, _manager.InformationMonitor);
            _informationPopup.Initialize();
            _informationPopup.RefreshState();

            _audioPopup = new AudioPopup(
                _preference,
                _manager.AudioMonitor,
                _manager.AudioRender,
                _view.AudioAssets,
                _config.audioTextUpdateRate);
            _audioPopup.Initialize();
            _audioPopup.RefreshState();

            _ramPopup = new RamPopup(
                _preference, 
                _manager.RamMonitor, 
                _manager.RamRender, 
                _view.RamAssets,
                _config.ramTextUpdateRate);
            _ramPopup.Initialize();
            _ramPopup.RefreshState();

            IsReady = true;
        }
    }
}