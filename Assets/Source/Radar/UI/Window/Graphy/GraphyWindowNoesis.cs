using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphy;
using Beast.Radar.Player;
using Cysharp.Threading.Tasks;
using Noesis;

namespace Beast.Radar.UI.Graphy
{
    public class GraphyWindowNoesis : Actor, IGraphyWindow, IReady
    {
        public bool IsReady { get; private set; }

        [Required] private readonly PlayerPreference _preference;
        [Required] private readonly UIConfig _uiConfig;
        [Required] private readonly IGraphyManager _graphyManager;
        [Required] private readonly GraphyConfig _config;
        [Required] private readonly IWindowManager _windowManager;

        private IntToStringConverter _intConverter;
        private FloatToStringConverter _floatConverter;

        private GraphyView _view;
        private GraphyFpsText _fpsText;

        public void Open()
        {
            if (_view == null)
            {
                _uiConfig.graphyAsset.Instantiate(out _view);
                _view.InitializeBrushes(_config.goodFpsColor, _config.cautionFpsColor, _config.criticalFpsColor);

                _windowManager.Attach(_view, 100);

                _intConverter = new IntToStringConverter(2000);
                _floatConverter = new FloatToStringConverter("D2");
                
                _fpsText = new GraphyFpsText(_config, _graphyManager.FpsMonitor, _view, _intConverter, _floatConverter);
                
                _fpsText.Initialize();
                
                _view.SetFpsTexture(_graphyManager.FpsRender.TextureGraph._material);
                
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
                _fpsText.UpdateText();
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