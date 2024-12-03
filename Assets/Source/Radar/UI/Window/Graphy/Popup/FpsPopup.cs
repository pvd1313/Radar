using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Beast.Claw.Graphy.Modules;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class FpsPopup
    {
        private readonly FpsMonitor _fpsMonitor;
        private readonly GraphyFpsAssets _assets;
        private readonly GraphyConfig _config;

        private readonly IntToStringConverter _intConverter;
        private readonly FloatToStringConverter _floatConverter;

        private int _updateRate;
        private int _frameCount;
        private float _deltaTime;
        private float _fps;
        private float _ms;
        
        public FpsPopup(
            GraphyConfig config,
            FpsMonitor monitor,
            GraphyFpsAssets assets,
            IntToStringConverter intConverter,
            FloatToStringConverter floatConverter)
        {
            _config = config;
            _fpsMonitor = monitor;
            _assets = assets;
            _intConverter = intConverter;
            _floatConverter = floatConverter;
        }

        public void Initialize()
        {
            _updateRate = _config.fpsTextUpdateRate;
        }

        public void UpdateText()
        {
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