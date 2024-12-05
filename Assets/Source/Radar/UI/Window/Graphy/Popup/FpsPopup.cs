using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Beast.Claw.Graphy.Modules;
using Beast.Claw.UI;
using Beast.Radar.Player;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class FpsPopup : GraphyPopup
    {
        private readonly FpsMonitor _monitor;
        private readonly FpsRender _render;
        private readonly GraphyFpsAssets _assets;
        private readonly PlayerPreference _preference;
        private readonly GraphyConfig _config;

        private IntToStringConverter _intConverter;
        private FloatToStringConverter _floatConverter;

        private int _updateRate;
        private int _frameCount;
        private float _deltaTime;
        private float _fps;
        private float _ms;

        public FpsPopup(PlayerPreference preference, GraphyConfig config, FpsMonitor monitor, FpsRender render ,GraphyFpsAssets assets)
        {
            _preference = preference;
            _config = config;
            _monitor = monitor;
            _render = render;
            _assets = assets;
        }

        public override void Initialize()
        {
            _intConverter = new IntToStringConverter(0, 2000);
            _floatConverter = new FloatToStringConverter("D2");

            _updateRate = _config.fpsTextUpdateRate;

            _assets.display.SetMaterial(_render.GetFpsMaterial());

            _assets.button.IsChecked = _preference.graphy.isFpsEnabled;
            
            _assets.button.Click += (_, _) =>
            {
                _preference.graphy.isFpsEnabled = !_preference.graphy.isFpsEnabled;
            
                RefreshState();
            };
        }

        public override void RefreshState()
        {
            _monitor.SetEnable(_preference.graphy.isFpsEnabled);
            
            _render.SetEnable(_preference.graphy.isFpsEnabled);
            
            _assets.view.Visibility = _preference.graphy.isFpsEnabled 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public override void Update()
        {
            if (!_preference.graphy.isFpsEnabled)
            {
                return;
            }

            _deltaTime += Time.unscaledDeltaTime;

            _frameCount++;

            if (_deltaTime > 1f / _updateRate)
            {
                _fps = _frameCount / _deltaTime;
                _ms = _deltaTime / _frameCount * 1000f;

                _assets.fps.Text = _intConverter.Convert((int)_fps);
                _setFpsRelatedTextColor(_assets.fps, _fps);

                _floatConverter.ToString(_ms, out string exponent, out string fractionWithDot);
                _assets.ms.Text = $"{exponent}{fractionWithDot}";
                _setFpsRelatedTextColor(_assets.ms, _fps);

                _assets.onePercentFps.Text = _intConverter.Convert(_monitor.OnePercentFPS);
                _setFpsRelatedTextColor(_assets.onePercentFps, _monitor.OnePercentFPS);

                _assets.textZero1PercentFps.Text = _intConverter.Convert(_monitor.Zero1PercentFps);
                _setFpsRelatedTextColor(_assets.textZero1PercentFps, _monitor.Zero1PercentFps);

                _assets.avg.Text = _intConverter.Convert(_monitor.AverageFPS);
                _setFpsRelatedTextColor(_assets.avg, _monitor.AverageFPS);

                _deltaTime = 0f;
                _frameCount = 0;
            }
        }
        
        private void _setFpsRelatedTextColor(TextBlock text, float fps)
        {
            int roundedFps = Mathf.RoundToInt(fps);

            if (roundedFps >= _config.goodFpsThreshold)
            {
                text.Foreground = _assets.brushes.good;
            }
            else if (roundedFps >= _config.cautionFpsThreshold)
            {
                text.Foreground = _assets.brushes.caution;
            }
            else
            {
                text.Foreground = _assets.brushes.critical;
            }
        }
    }
}