using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class FpsRender : GraphyMonitor
    {
        private readonly GraphyConfig _config;
        private readonly FpsMonitor _monitor;
        
        private GraphyTexture _textureGraph;
        private int _resolution;
        private int[] _fpsArray;
        private int _highestFps;

        public FpsRender(GraphyConfig config, FpsMonitor monitor)
        {
            _config = config;
            _monitor = monitor;
        }

        public override void Initialize()
        {
            _resolution = _config.fpsGraphResolution;

            _textureGraph = new GraphyTexture(_config);
            _textureGraph.InitializeShader();
            
            _createPoints();
        }

        public override void Update()
        {
            if (!IsEnabled)
            {
                return;
            }
            
            short fps = (short)(1 / Time.unscaledDeltaTime);

            int currentMaxFps = 0;

            for (int i = 0; i <= _resolution - 1; i++)
            {
                if (i >= _resolution - 1)
                {
                    _fpsArray[i] = fps;
                }
                else
                {
                    _fpsArray[i] = _fpsArray[i + 1];
                }

                if (currentMaxFps < _fpsArray[i])
                {
                    currentMaxFps = _fpsArray[i];
                }
            }

            _highestFps = _highestFps < 1 || _highestFps <= currentMaxFps ? currentMaxFps : _highestFps - 1;

            _highestFps = _highestFps > 0 ? _highestFps : 1;

            if (_textureGraph.shaderArrayValues == null)
            {
                _fpsArray = new int[_resolution];
                _textureGraph.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                _textureGraph.shaderArrayValues[i] = _fpsArray[i] / (float)_highestFps;
            }

            _textureGraph.UpdatePoints();

            _textureGraph.average = (float)_monitor.AverageFPS / _highestFps;
            _textureGraph.UpdateAverage();

            _textureGraph.goodThreshold = (float)_config.goodFpsThreshold / _highestFps;
            _textureGraph.cautionThreshold = (float)_config.cautionFpsThreshold / _highestFps;
            _textureGraph.UpdateThresholds();
            
            _textureGraph.Blit();
        }

        public Material GetFpsMaterial()
        {
            return _textureGraph.material;
        }

        private void _createPoints()
        {
            if (_textureGraph.shaderArrayValues == null || _fpsArray.Length != _resolution)
            {
                _fpsArray = new int[_resolution];
                _textureGraph.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i < _resolution; i++)
            {
                _textureGraph.shaderArrayValues[i] = 0;
            }

            _textureGraph.goodColor = _config.goodFpsColor;
            _textureGraph.cautionColor = _config.cautionFpsColor;
            _textureGraph.criticalColor = _config.criticalFpsColor;

            _textureGraph.UpdateColors();

            _textureGraph.UpdateArrayValuesLength();
        }

        public void Dispose()
        {
            _textureGraph.Dispose();
        }
    }
}