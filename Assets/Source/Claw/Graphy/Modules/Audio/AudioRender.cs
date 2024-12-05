using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class AudioRender : GraphyMonitor
    {
        private readonly GraphyConfig _config;
        private readonly AudioMonitor _monitor;

        private GraphyTexture _textureGraphCurrent;
        private GraphyTexture _textureGraphPrevious;
        private int _resolution;

        private float[] _renderArrayCurrent;
        private float[] _renderArrayPrevious;
        
        public AudioRender(GraphyConfig config, AudioMonitor monitor)
        {
            _config = config;
            _monitor = monitor;
        }

        public override void Initialize()
        {
            _resolution = _config.audioGraphResolution;

            _textureGraphCurrent = new GraphyTexture(_config);
            _textureGraphCurrent.InitializeShader();

            _textureGraphPrevious = new GraphyTexture(_config);
            _textureGraphPrevious.InitializeShader();
            
            _createPoints();
        }

        public override void Update()
        {
            if (!IsEnabled)
            {
                return;
            }
            
            int incrementPerIteration = Mathf.FloorToInt(_monitor.SpectrumCurrent.Length / (float)_resolution);
            
            _updateCurrentValue(incrementPerIteration);
            
            _updateHighValue(incrementPerIteration);
        }

        public Material GetCurrentMaterial()
        {
            return _textureGraphCurrent.material;
        }

        public Material getPreviousMaterial()
        {
            return _textureGraphPrevious.material;
        }
        
        private void _updateHighValue(int incrementPerIteration)
        {
            for (int i = 0; i <= _resolution - 1; i++)
            {
                float currentValue = 0;

                for (int j = 0; j < incrementPerIteration; j++)
                {
                    currentValue += _monitor.SpectrumPrevious[i * incrementPerIteration + j];
                }

                // Uses 3 values for each bar to accomplish that look

                if ((i + 1) % 3 == 0 && i > 1)
                {
                    float value =
                    (
                        _monitor.DBNormalized(_monitor.Lin2dB(currentValue / incrementPerIteration))
                        + _renderArrayPrevious[i - 1]
                        + _renderArrayPrevious[i - 2]
                    ) / 3;

                    _renderArrayPrevious[i] = value;
                    _renderArrayPrevious[i - 1] = value;
                    _renderArrayPrevious[i - 2] =
                        -1; // Always set the third one to -1 to leave gaps in the graph and improve readability
                }
                else
                {
                    _renderArrayPrevious[i] =
                        _monitor.DBNormalized(_monitor.Lin2dB(currentValue / incrementPerIteration));
                }
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                _textureGraphPrevious.shaderArrayValues[i] = _renderArrayPrevious[i];
            }

            _textureGraphPrevious.UpdatePoints();
            
            _textureGraphPrevious.Blit();
        }

        private void _updateCurrentValue(int incrementPerIteration)
        {
            for (int i = 0; i <= _resolution - 1; i++)
            {
                float currentValue = 0;

                for (int j = 0; j < incrementPerIteration; j++)
                {
                    currentValue += _monitor.SpectrumCurrent[i * incrementPerIteration + j];
                }

                // Uses 3 values for each bar to accomplish that look

                if ((i + 1) % 3 == 0 && i > 1)
                {
                    float value =
                    (
                        _monitor.DBNormalized(_monitor.Lin2dB(currentValue / incrementPerIteration))
                        + _renderArrayCurrent[i - 1]
                        + _renderArrayCurrent[i - 2]
                    ) / 3;

                    _renderArrayCurrent[i] = value;
                    _renderArrayCurrent[i - 1] = value;
                    _renderArrayCurrent[i - 2] =
                        -1; // Always set the third one to -1 to leave gaps in the graph and improve readability
                }
                else
                {
                    _renderArrayCurrent[i] =
                        _monitor.DBNormalized(_monitor.Lin2dB(currentValue / incrementPerIteration));
                }
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                _textureGraphCurrent.shaderArrayValues[i] = _renderArrayCurrent[i];
            }

            _textureGraphCurrent.UpdatePoints();
            
            _textureGraphCurrent.Blit();
        }

        private void _createPoints()
        {
            // Init Arrays
            if (_textureGraphCurrent.shaderArrayValues == null || _textureGraphCurrent.shaderArrayValues.Length != _resolution)
            {
                _renderArrayCurrent = new float[_resolution];
                _renderArrayPrevious = new float[_resolution];
                _textureGraphCurrent.shaderArrayValues = new float[_resolution];
                _textureGraphPrevious.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i < _resolution; i++)
            {
                _textureGraphCurrent.shaderArrayValues[i] = 0;
                _textureGraphPrevious.shaderArrayValues[i] = 0;
            }

            // Color
            _textureGraphCurrent.goodColor = _config.audioColor;
            _textureGraphCurrent.cautionColor = _config.audioColor;
            _textureGraphCurrent.criticalColor = _config.audioColor;
            _textureGraphCurrent.UpdateColors();

            _textureGraphPrevious.goodColor = _config.audioColor;
            _textureGraphPrevious.cautionColor = _config.audioColor;
            _textureGraphPrevious.criticalColor = _config.audioColor;
            _textureGraphPrevious.UpdateColors();

            // Threshold
            _textureGraphCurrent.goodThreshold = 0;
            _textureGraphCurrent.cautionThreshold = 0;
            _textureGraphCurrent.UpdateThresholds();

            _textureGraphPrevious.goodThreshold = 0;
            _textureGraphPrevious.cautionThreshold = 0;
            _textureGraphPrevious.UpdateThresholds();

            // Update Array
            _textureGraphCurrent.UpdateArrayValuesLength();
            _textureGraphPrevious.UpdateArrayValuesLength();

            // Average
            _textureGraphCurrent.average = 0;
            _textureGraphCurrent.UpdateAverage();

            _textureGraphPrevious.average = 0;
            _textureGraphPrevious.UpdateAverage();
        }
        

        public void Dispose()
        {
            _textureGraphCurrent.Dispose();
            _textureGraphPrevious.Dispose();
        }
    }
}