using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI.Graphy
{
    public class GraphyFpsText
    {
        private GraphyFpsTextAssets _textAssets;

        private IntToStringConverter _intConverter;
        private FloatToStringConverter _floatConverter;

        private GraphyFpsMonitor _fpsMonitor;


        private int _updateRate = 4; // 4 updates per sec.

        private int _frameCount = 0;

        private float _deltaTime = 0f;

        private float _fps = 0f;

        private float _ms = 0f;



        public GraphyFpsText(
            GraphyFpsMonitor monitor,
            GraphyFpsTextAssets textAssets,
            IntToStringConverter intConverter,
            FloatToStringConverter floatConverter)
        {
            _fpsMonitor = monitor;
            _textAssets = textAssets;
            _intConverter = intConverter;
            _floatConverter = floatConverter;
        }
        

        public void UpdateText()
        {
            _deltaTime += Time.unscaledDeltaTime;

            _frameCount++;

            // Only update texts 'm_updateRate' times per second

            if (_deltaTime > 1f / _updateRate)
            {
                _fps = _frameCount / _deltaTime;
                _ms = _deltaTime / _frameCount * 1000f;

                // Update fps
                _textAssets.fps.Text = _intConverter.Convert((int)_fps);
                SetFpsRelatedTextColor(_textAssets.fps, _fps);

                // Update ms
                _floatConverter.ToString(_ms, out string exponent, out string fractionWithDot);
                _textAssets.ms.Text = $"{exponent}{fractionWithDot}";
                SetFpsRelatedTextColor(_textAssets.ms, _fps);

                 //Update 1% fps
                 _textAssets.onePercentFps.Text = _intConverter.Convert(_fpsMonitor.OnePercentFPS);
                SetFpsRelatedTextColor(_textAssets.onePercentFps, _fpsMonitor.OnePercentFPS);

                // Update 0.1% fps
                _textAssets.textZero1PercentFps.Text = _intConverter.Convert(_fpsMonitor.Zero1PercentFps);
                SetFpsRelatedTextColor(_textAssets.textZero1PercentFps, _fpsMonitor.Zero1PercentFps );

                // Update avg fps
                _textAssets.avg.Text = _intConverter.Convert(_fpsMonitor.AverageFPS);
                SetFpsRelatedTextColor(_textAssets.avg, _fpsMonitor.AverageFPS);

                // Reset variables
                _deltaTime = 0f;
                _frameCount = 0;
            }
        }


        public void UpdateParameters(int updateRate)
        {
            //_updateRate = m_graphyManager.FpsTextUpdateRate;
            _updateRate = updateRate;
        }


        #region Methods -> Private

        /// <summary>
        /// Assigns color to a text according to their fps numeric value and
        /// the colors specified in the 3 categories (Good, Caution, Critical).
        /// </summary>
        /// 
        /// <param name="text">
        /// UI Text component to change its color
        /// </param>
        /// 
        /// <param name="fps">
        /// Numeric fps value
        /// </param>
        private void SetFpsRelatedTextColor(TextBlock text, float fps)
        {
            /*int roundedFps = Mathf.RoundToInt(fps);

            if (roundedFps >= m_graphyManager.GoodFPSThreshold)
            {
                text.color = m_graphyManager.GoodFPSColor;
            }
            else if (roundedFps >= m_graphyManager.CautionFPSThreshold)
            {
                text.color = m_graphyManager.CautionFPSColor;
            }
            else
            {
                text.color = m_graphyManager.CriticalFPSColor;
            }*/
        }

        #endregion
    }
}