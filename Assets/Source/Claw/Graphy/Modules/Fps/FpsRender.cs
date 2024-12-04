using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class FpsRender
    {
        public GraphyTexture TextureGraph { get; private set; }

        private readonly GraphyConfig _config;
        private readonly FpsMonitor _monitor;
        
        private int _resolution;
        private int[] _fpsArray;
        private int _highestFps;

        public FpsRender(GraphyConfig config, FpsMonitor monitor)
        {
            _config = config;
            _monitor = monitor;
        }

        public void Initialize()
        {
            TextureGraph = new GraphyTexture(_config);
            TextureGraph.InitializeShader();

            _resolution = _config.fpsGraphResolution;

            CreatePoints();
        }


        public void Update()
        {
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

            if (TextureGraph.shaderArrayValues == null)
            {
                _fpsArray = new int[_resolution];
                TextureGraph.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                TextureGraph.shaderArrayValues[i] = _fpsArray[i] / (float)_highestFps;
            }

            TextureGraph.UpdatePoints();

            TextureGraph.average = (float)_monitor.AverageFPS / _highestFps;
            TextureGraph.UpdateAverage();

            TextureGraph.goodThreshold = (float)_config.goodFpsThreshold / _highestFps;
            TextureGraph.cautionThreshold = (float)_config.cautionFpsThreshold / _highestFps;
            TextureGraph.UpdateThresholds();
            
            TextureGraph.Blit();
        }

        private void CreatePoints()
        {
            if (TextureGraph.shaderArrayValues == null || _fpsArray.Length != _resolution)
            {
                _fpsArray = new int[_resolution];
                TextureGraph.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i < _resolution; i++)
            {
                TextureGraph.shaderArrayValues[i] = 0;
            }

            TextureGraph.goodColor = _config.goodFpsColor;
            TextureGraph.cautionColor = _config.cautionFpsColor;
            TextureGraph.criticalColor = _config.criticalFpsColor;

            TextureGraph.UpdateColors();

            TextureGraph.UpdateArrayValuesLength();
        }

        public void Dispose()
        {
            TextureGraph.Dispose();
        }
    }
}