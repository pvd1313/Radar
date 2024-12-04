using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class AudioRender
    {
        public GraphyTexture TextureGraphLow { get; private set; }
        public GraphyTexture TextureGraphHigh { get; private set; }

        private readonly GraphyConfig _config;
        private readonly AudioMonitor _monitor;

        private int _resolution;

        private float[] _renderArrayLow;
        private float[] _renderArrayHigh;
        
        
        public AudioRender(GraphyConfig config, AudioMonitor monitor)
        {
            _config = config;
            _monitor = monitor;
        }

        public void Initialize()
        {
            TextureGraphLow = new GraphyTexture(_config);
            TextureGraphLow.InitializeShader();

            TextureGraphHigh = new GraphyTexture(_config);
            TextureGraphHigh.InitializeShader();

            _resolution = _config.audioGraphResolution;

            CreatePoints();
        }

        public void Update()
        {
            int incrementPerIteration = Mathf.FloorToInt(_monitor.Spectrum.Length / (float)_resolution);
            
            UpdateCurrentValue(incrementPerIteration);
            
            UpdateHighValue(incrementPerIteration);
        }

        private void UpdateHighValue(int incrementPerIteration)
        {
            for (int i = 0; i <= _resolution - 1; i++)
            {
                float currentValue = 0;

                for (int j = 0; j < incrementPerIteration; j++)
                {
                    currentValue += _monitor.SpectrumHighestValues[i * incrementPerIteration + j];
                }

                // Uses 3 values for each bar to accomplish that look

                if ((i + 1) % 3 == 0 && i > 1)
                {
                    float value =
                    (
                        _monitor.dBNormalized(_monitor.lin2dB(currentValue / incrementPerIteration))
                        + _renderArrayHigh[i - 1]
                        + _renderArrayHigh[i - 2]
                    ) / 3;

                    _renderArrayHigh[i] = value;
                    _renderArrayHigh[i - 1] = value;
                    _renderArrayHigh[i - 2] =
                        -1; // Always set the third one to -1 to leave gaps in the graph and improve readability
                }
                else
                {
                    _renderArrayHigh[i] =
                        _monitor.dBNormalized(_monitor.lin2dB(currentValue / incrementPerIteration));
                }
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                TextureGraphHigh.shaderArrayValues[i] = _renderArrayHigh[i];
            }

            TextureGraphHigh.UpdatePoints();


            TextureGraphHigh.Blit();
        }

        private void UpdateCurrentValue(int incrementPerIteration)
        {
            for (int i = 0; i <= _resolution - 1; i++)
            {
                float currentValue = 0;

                for (int j = 0; j < incrementPerIteration; j++)
                {
                    currentValue += _monitor.Spectrum[i * incrementPerIteration + j];
                }

                // Uses 3 values for each bar to accomplish that look

                if ((i + 1) % 3 == 0 && i > 1)
                {
                    float value =
                    (
                        _monitor.dBNormalized(_monitor.lin2dB(currentValue / incrementPerIteration))
                        + _renderArrayLow[i - 1]
                        + _renderArrayLow[i - 2]
                    ) / 3;

                    _renderArrayLow[i] = value;
                    _renderArrayLow[i - 1] = value;
                    _renderArrayLow[i - 2] =
                        -1; // Always set the third one to -1 to leave gaps in the graph and improve readability
                }
                else
                {
                    _renderArrayLow[i] =
                        _monitor.dBNormalized(_monitor.lin2dB(currentValue / incrementPerIteration));
                }
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                TextureGraphLow.shaderArrayValues[i] = _renderArrayLow[i];
            }

            TextureGraphLow.UpdatePoints();
            TextureGraphLow.Blit();
        }

        private void CreatePoints()
        {
            // Init Arrays
            if (TextureGraphLow.shaderArrayValues == null || TextureGraphLow.shaderArrayValues.Length != _resolution)
            {
                _renderArrayLow = new float[_resolution];
                _renderArrayHigh = new float[_resolution];
                TextureGraphLow.shaderArrayValues = new float[_resolution];
                TextureGraphHigh.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i < _resolution; i++)
            {
                TextureGraphLow.shaderArrayValues[i] = 0;
                TextureGraphHigh.shaderArrayValues[i] = 0;
            }

            // Color
            TextureGraphLow.goodColor = _config.audioColor;
            TextureGraphLow.cautionColor = _config.audioColor;
            TextureGraphLow.criticalColor = _config.audioColor;
            TextureGraphLow.UpdateColors();

            TextureGraphHigh.goodColor = _config.audioColor;
            TextureGraphHigh.cautionColor = _config.audioColor;
            TextureGraphHigh.criticalColor = _config.audioColor;
            TextureGraphHigh.UpdateColors();

            // Threshold
            TextureGraphLow.goodThreshold = 0;
            TextureGraphLow.cautionThreshold = 0;
            TextureGraphLow.UpdateThresholds();

            TextureGraphHigh.goodThreshold = 0;
            TextureGraphHigh.cautionThreshold = 0;
            TextureGraphHigh.UpdateThresholds();

            // Update Array
            TextureGraphLow.UpdateArrayValuesLength();
            TextureGraphHigh.UpdateArrayValuesLength();

            // Average
            TextureGraphLow.average = 0;
            TextureGraphLow.UpdateAverage();

            TextureGraphHigh.average = 0;
            TextureGraphHigh.UpdateAverage();
        }
        

        public void Dispose()
        {
            TextureGraphLow.Dispose();
            TextureGraphHigh.Dispose();
        }
    }
}