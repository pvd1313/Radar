using Beast.Claw.GameLoop;
using UnityEngine;
using System;

namespace Beast.Claw.Graphy
{
    public class GraphyFpsMonitor : Actor
    {
        public short CurrentFPS { get; private set; } = 0;
        public short AverageFPS { get; private set; } = 0;
        public short OnePercentFPS { get; private set; } = 0;
        public short Zero1PercentFps { get; private set; } = 0;

        private short[] _fpsSamples;
        private short[] _fpsSamplesSorted;
        private short _fpsSamplesCapacity = 1024;
        private short _onePercentSamples = 10;
        private short _zero1PercentSamples = 1;
        private short _fpsSamplesCount = 0;
        private short _indexSample = 0;

        private float _unscaledDeltaTime = 0f;

        public void Init()
        {
            _fpsSamples = new short[_fpsSamplesCapacity];
            _fpsSamplesSorted = new short[_fpsSamplesCapacity];

            UpdateParameters();
        }

        
        public  void Update()
        {
            _unscaledDeltaTime = Time.unscaledDeltaTime;

            // Update fps and ms

            CurrentFPS = (short)(Mathf.RoundToInt(1f / _unscaledDeltaTime));

            // Update avg fps

            uint averageAddedFps = 0;

            _indexSample++;

            if (_indexSample >= _fpsSamplesCapacity) _indexSample = 0;

            _fpsSamples[_indexSample] = CurrentFPS;

            if (_fpsSamplesCount < _fpsSamplesCapacity)
            {
                _fpsSamplesCount++;
            }

            for (int i = 0; i < _fpsSamplesCount; i++)
            {
                averageAddedFps += (uint)_fpsSamples[i];
            }

            AverageFPS = (short)((float)averageAddedFps / (float)_fpsSamplesCount);

            // Update percent lows

            _fpsSamples.CopyTo(_fpsSamplesSorted, 0);

            /*
             * TODO: Find a faster way to do this.
             *      We can probably avoid copying the full array every time
             *      and insert the new item already sorted in the list.
             */
            Array.Sort(_fpsSamplesSorted,
                (x, y) => x.CompareTo(y)); // The lambda expression avoids garbage generation

            bool zero1PercentCalculated = false;

            uint totalAddedFps = 0;

            short samplesToIterateThroughForOnePercent = _fpsSamplesCount < _onePercentSamples
                ? _fpsSamplesCount
                : _onePercentSamples;

            short samplesToIterateThroughForZero1Percent = _fpsSamplesCount < _zero1PercentSamples
                ? _fpsSamplesCount
                : _zero1PercentSamples;

            short sampleToStartIn = (short)(_fpsSamplesCapacity - _fpsSamplesCount);

            for (short i = sampleToStartIn; i < sampleToStartIn + samplesToIterateThroughForOnePercent; i++)
            {
                totalAddedFps += (ushort)_fpsSamplesSorted[i];

                if (!zero1PercentCalculated && i >= samplesToIterateThroughForZero1Percent - 1)
                {
                    zero1PercentCalculated = true;

                    Zero1PercentFps = (short)((float)totalAddedFps / (float)_zero1PercentSamples);
                }
            }

            OnePercentFPS = (short)((float)totalAddedFps / (float)_onePercentSamples);
        }
        
        public void UpdateParameters()
        {
            _onePercentSamples = (short)(_fpsSamplesCapacity / 100);
            _zero1PercentSamples = (short)(_fpsSamplesCapacity / 1000);
        }

    }
}
