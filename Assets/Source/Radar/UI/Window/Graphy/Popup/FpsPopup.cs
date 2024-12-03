using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Beast.Claw.Graphy.Modules;
using Beast.Radar.Player;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class FpsPopup
    {
        private readonly FpsMonitor _fpsMonitor;
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

        public FpsPopup(PlayerPreference preference, GraphyConfig config, FpsMonitor monitor, GraphyFpsAssets assets)
        {
            _preference = preference;
            _config = config;
            _fpsMonitor = monitor;
            _assets = assets;
        }

        public void Initialize()
        {
            _intConverter = new IntToStringConverter(2000);
            _floatConverter = new FloatToStringConverter("D2");

            _updateRate = _config.fpsTextUpdateRate;

            _assets.button.Click += (_, _) =>
            {
                _preference.graphy.isFpsEnabled = !_preference.graphy.isFpsEnabled;
                SetState();
            };
        }

        public void SetState()
        {
            _assets.view.Visibility = _preference.graphy.isFpsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdateText()
        {
            if (!_preference.graphy.isAudioEnabled)
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
                SetFpsRelatedTextColor(_assets.fps, _fps);

                _floatConverter.ToString(_ms, out string exponent, out string fractionWithDot);
                _assets.ms.Text = $"{exponent}{fractionWithDot}";
                SetFpsRelatedTextColor(_assets.ms, _fps);

                _assets.onePercentFps.Text = _intConverter.Convert(_fpsMonitor.OnePercentFPS);
                SetFpsRelatedTextColor(_assets.onePercentFps, _fpsMonitor.OnePercentFPS);

                _assets.textZero1PercentFps.Text = _intConverter.Convert(_fpsMonitor.Zero1PercentFps);
                SetFpsRelatedTextColor(_assets.textZero1PercentFps, _fpsMonitor.Zero1PercentFps);

                _assets.avg.Text = _intConverter.Convert(_fpsMonitor.AverageFPS);
                SetFpsRelatedTextColor(_assets.avg, _fpsMonitor.AverageFPS);

                _deltaTime = 0f;
                _frameCount = 0;
            }
        }


        private void SetFpsRelatedTextColor(TextBlock text, float fps)
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