using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Beast.Claw.Graphy.Fps;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI.Graphy
{
    public class GraphyFpsText
    {
        private readonly GraphyFpsMonitor _fpsMonitor;
        private readonly GraphyView _view;
        private readonly GraphyConfig _config;

        private readonly IntToStringConverter _intConverter;
        private readonly FloatToStringConverter _floatConverter;

        private int _updateRate;
        private int _frameCount;
        private float _deltaTime;
        private float _fps;
        private float _ms;


        public GraphyFpsText(
            GraphyConfig config,
            GraphyFpsMonitor monitor,
            GraphyView view,
            IntToStringConverter intConverter,
            FloatToStringConverter floatConverter)
        {
            _config = config;
            _fpsMonitor = monitor;
            _view = view;
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

                _view.FpsTextAssets.fps.Text = _intConverter.Convert((int)_fps);
                SetFpsRelatedTextColor(_view.FpsTextAssets.fps, _fps);

                _floatConverter.ToString(_ms, out string exponent, out string fractionWithDot);
                _view.FpsTextAssets.ms.Text = $"{exponent}{fractionWithDot}";
                SetFpsRelatedTextColor(_view.FpsTextAssets.ms, _fps);

                _view.FpsTextAssets.onePercentFps.Text = _intConverter.Convert(_fpsMonitor.OnePercentFPS);
                SetFpsRelatedTextColor(_view.FpsTextAssets.onePercentFps, _fpsMonitor.OnePercentFPS);

                _view.FpsTextAssets.textZero1PercentFps.Text = _intConverter.Convert(_fpsMonitor.Zero1PercentFps);
                SetFpsRelatedTextColor(_view.FpsTextAssets.textZero1PercentFps, _fpsMonitor.Zero1PercentFps);

                _view.FpsTextAssets.avg.Text = _intConverter.Convert(_fpsMonitor.AverageFPS);
                SetFpsRelatedTextColor(_view.FpsTextAssets.avg, _fpsMonitor.AverageFPS);

                _deltaTime = 0f;
                _frameCount = 0;
            }
        }


        private void SetFpsRelatedTextColor(TextBlock text, float fps)
        {
            int roundedFps = Mathf.RoundToInt(fps);

            if (roundedFps >= _config.goodFpsThreshold)
            {
                text.Foreground = _view.Brushes.good;
            }
            else if (roundedFps >= _config.cautionFpsThreshold)
            {
                text.Foreground = _view.Brushes.caution;
            }
            else
            {
                text.Foreground = _view.Brushes.critical;
            }
        }
    }
}