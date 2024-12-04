using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class AudioRender
    {
        public GraphyTexture TextureGraphCurrent { get; private set; }
        public GraphyTexture TextureGraphPrevious { get; private set; }

        private readonly GraphyConfig _config;
        private readonly AudioMonitor _monitor;

        private int _resolution;

        private float[] _renderArrayCurrent;
        private float[] _renderArrayPrevious;
        
        public AudioRender(GraphyConfig config, AudioMonitor monitor)
        {
            _config = config;
            _monitor = monitor;
        }

        public void Initialize()
        {
            _resolution = _config.audioGraphResolution;

            TextureGraphCurrent = new GraphyTexture(_config);
            TextureGraphCurrent.InitializeShader();

            TextureGraphPrevious = new GraphyTexture(_config);
            TextureGraphPrevious.InitializeShader();
            
            _createPoints();
        }

        public void Update()
        {
            int incrementPerIteration = Mathf.FloorToInt(_monitor.SpectrumCurrent.Length / (float)_resolution);
            
            _updateCurrentValue(incrementPerIteration);
            
            _updateHighValue(incrementPerIteration);
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
                TextureGraphPrevious.shaderArrayValues[i] = _renderArrayPrevious[i];
            }

            TextureGraphPrevious.UpdatePoints();
            
            TextureGraphPrevious.Blit();
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
                TextureGraphCurrent.shaderArrayValues[i] = _renderArrayCurrent[i];
            }

            TextureGraphCurrent.UpdatePoints();
            
            TextureGraphCurrent.Blit();
        }

        private void _createPoints()
        {
            // Init Arrays
            if (TextureGraphCurrent.shaderArrayValues == null || TextureGraphCurrent.shaderArrayValues.Length != _resolution)
            {
                _renderArrayCurrent = new float[_resolution];
                _renderArrayPrevious = new float[_resolution];
                TextureGraphCurrent.shaderArrayValues = new float[_resolution];
                TextureGraphPrevious.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i < _resolution; i++)
            {
                TextureGraphCurrent.shaderArrayValues[i] = 0;
                TextureGraphPrevious.shaderArrayValues[i] = 0;
            }

            // Color
            TextureGraphCurrent.goodColor = _config.audioColor;
            TextureGraphCurrent.cautionColor = _config.audioColor;
            TextureGraphCurrent.criticalColor = _config.audioColor;
            TextureGraphCurrent.UpdateColors();

            TextureGraphPrevious.goodColor = _config.audioColor;
            TextureGraphPrevious.cautionColor = _config.audioColor;
            TextureGraphPrevious.criticalColor = _config.audioColor;
            TextureGraphPrevious.UpdateColors();

            // Threshold
            TextureGraphCurrent.goodThreshold = 0;
            TextureGraphCurrent.cautionThreshold = 0;
            TextureGraphCurrent.UpdateThresholds();

            TextureGraphPrevious.goodThreshold = 0;
            TextureGraphPrevious.cautionThreshold = 0;
            TextureGraphPrevious.UpdateThresholds();

            // Update Array
            TextureGraphCurrent.UpdateArrayValuesLength();
            TextureGraphPrevious.UpdateArrayValuesLength();

            // Average
            TextureGraphCurrent.average = 0;
            TextureGraphCurrent.UpdateAverage();

            TextureGraphPrevious.average = 0;
            TextureGraphPrevious.UpdateAverage();
        }
        

        public void Dispose()
        {
            TextureGraphCurrent.Dispose();
            TextureGraphPrevious.Dispose();
        }
    }
}