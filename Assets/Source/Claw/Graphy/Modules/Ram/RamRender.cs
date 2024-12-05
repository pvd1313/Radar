using UnityEngine;

namespace Beast.Claw.Graphy.Modules.Ram
{
    public class RamRender : GraphyMonitor
    {
        private readonly GraphyConfig _config;
        private readonly RamMonitor _monitor;

        private GraphyTexture _textureGraphAlloc;
        private GraphyTexture _textureGraphReserved;
        private GraphyTexture _textureGraphMono;

        private float[] _allocatedArray;
        private float[] _reservedArray;
        private float[] _monoArray;

        private int _resolution;
        private float _highestMemory;

        public RamRender(GraphyConfig config, RamMonitor monitor)
        {
            _config = config;
            _monitor = monitor;
        }

        public override void Initialize()
        {
            _resolution = _config.ramGraphResolution;

            _textureGraphAlloc = new GraphyTexture(_config);
            _textureGraphAlloc.InitializeShader();

            _textureGraphReserved = new GraphyTexture(_config);
            _textureGraphReserved.InitializeShader();

            _textureGraphMono = new GraphyTexture(_config);
            _textureGraphMono.InitializeShader();

            _createPoints();
        }


        public override void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            float allocatedMemory = _monitor.AllocatedRam;
            float reservedMemory = _monitor.ReservedRam;
            float monoMemory = _monitor.MonoRam;

            _highestMemory = 0;

            for (int i = 0; i <= _resolution - 1; i++)
            {
                if (i >= _resolution - 1)
                {
                    _allocatedArray[i] = allocatedMemory;
                    _reservedArray[i] = reservedMemory;
                    _monoArray[i] = monoMemory;
                }
                else
                {
                    _allocatedArray[i] = _allocatedArray[i + 1];
                    _reservedArray[i] = _reservedArray[i + 1];
                    _monoArray[i] = _monoArray[i + 1];
                }

                if (_highestMemory < _reservedArray[i])
                {
                    _highestMemory = _reservedArray[i];
                }
            }

            for (int i = 0; i <= _resolution - 1; i++)
            {
                _textureGraphAlloc.shaderArrayValues[i] = _allocatedArray[i] / _highestMemory;

                _textureGraphReserved.shaderArrayValues[i] = _reservedArray[i] / _highestMemory;

                _textureGraphMono.shaderArrayValues[i] = _monoArray[i] / _highestMemory;
            }

            _textureGraphAlloc.UpdatePoints();
            _textureGraphReserved.UpdatePoints();
            _textureGraphMono.UpdatePoints();
            
            _textureGraphAlloc.Blit();
            _textureGraphReserved.Blit();
            _textureGraphMono.Blit();
        }

        public Material GetAllocatedMaterial()
        {
            return _textureGraphAlloc.material;
        }

        public Material GetMonoMaterial()
        {
            return _textureGraphMono.material;
        }

        public Material GetReservedMaterial()
        {
            return _textureGraphReserved.material;
        }

        private void _createPoints()
        {
            if (_textureGraphAlloc.shaderArrayValues == null ||
                _textureGraphAlloc.shaderArrayValues.Length != _resolution)
            {
                _allocatedArray = new float[_resolution];
                _reservedArray = new float[_resolution];
                _monoArray = new float[_resolution];

                _textureGraphAlloc.shaderArrayValues = new float[_resolution];
                _textureGraphReserved.shaderArrayValues = new float[_resolution];
                _textureGraphMono.shaderArrayValues = new float[_resolution];
            }

            for (int i = 0; i < _resolution; i++)
            {
                _textureGraphAlloc.shaderArrayValues[i] = 0;
                _textureGraphReserved.shaderArrayValues[i] = 0;
                _textureGraphMono.shaderArrayValues[i] = 0;
            }

            // Initialize the material values

            // Colors

            _textureGraphAlloc.goodColor = _config.allocatedRamColor;
            _textureGraphAlloc.cautionColor = _config.allocatedRamColor;
            _textureGraphAlloc.criticalColor = _config.allocatedRamColor;

            _textureGraphAlloc.UpdateColors();

            _textureGraphReserved.goodColor = _config.reservedRamColor;
            _textureGraphReserved.cautionColor = _config.reservedRamColor;
            _textureGraphReserved.criticalColor = _config.reservedRamColor;

            _textureGraphReserved.UpdateColors();

            _textureGraphMono.goodColor = _config.monoRamColor;
            _textureGraphMono.cautionColor = _config.monoRamColor;
            _textureGraphMono.criticalColor = _config.monoRamColor;

            _textureGraphMono.UpdateColors();

            // Thresholds

            _textureGraphAlloc.goodThreshold = 0;
            _textureGraphAlloc.cautionThreshold = 0;
            _textureGraphAlloc.UpdateThresholds();

            _textureGraphReserved.goodThreshold = 0;
            _textureGraphReserved.cautionThreshold = 0;
            _textureGraphReserved.UpdateThresholds();

            _textureGraphMono.goodThreshold = 0;
            _textureGraphMono.cautionThreshold = 0;
            _textureGraphMono.UpdateThresholds();

            _textureGraphAlloc.UpdateArrayValuesLength();
            _textureGraphReserved.UpdateArrayValuesLength();
            _textureGraphMono.UpdateArrayValuesLength();

            // Average

            _textureGraphAlloc.average = 0;
            _textureGraphReserved.average = 0;
            _textureGraphMono.average = 0;

            _textureGraphAlloc.UpdateAverage();
            _textureGraphReserved.UpdateAverage();
            _textureGraphMono.UpdateAverage();
        }

        public void Dispose()
        {
            _textureGraphAlloc.Dispose();
            _textureGraphReserved.Dispose();
            _textureGraphMono.Dispose();
        }
    }
}