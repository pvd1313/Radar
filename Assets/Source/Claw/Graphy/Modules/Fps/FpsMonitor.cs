using System;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class FpsMonitor : GraphyMonitor
    {
        public short CurrentFPS { get; private set; }
        public short AverageFPS { get; private set; }
        public short OnePercentFPS { get; private set; }
        public short Zero1PercentFps { get; private set; }

        private const short FPS_SAMPLES_CAPACITY = 1024;

        private short[] _fpsSamples;
        private short[] _sortedSamples;
        private short _onePercentSamples;
        private short _zero1PercentSamples;
        private short _fpsSamplesCount;
        private short _indexSample;

        private float _unscaledDeltaTime;
        
        public override void Initialize()
        {
            _fpsSamples = new short[FPS_SAMPLES_CAPACITY];
            _sortedSamples = new short[FPS_SAMPLES_CAPACITY];
            _updateParameters();
        }

        public override void Update()
        {
            if (!IsEnabled)
            {
                return;
            }
            
            _unscaledDeltaTime = Time.unscaledDeltaTime;

            CurrentFPS = (short)Mathf.RoundToInt(1f / _unscaledDeltaTime);

            _indexSample = (short)((_indexSample + 1) % FPS_SAMPLES_CAPACITY);
            
            short oldSample = _fpsSamples[_indexSample];

            _fpsSamples[_indexSample] = CurrentFPS;

            if (_fpsSamplesCount < FPS_SAMPLES_CAPACITY)
            {
                _fpsSamplesCount++;
            }

            _removeFromSortedArray(oldSample);

            _insertIntoSortedArray(CurrentFPS);

            long totalFPS = 0;
            for (int i = 0; i < _fpsSamplesCount; i++)
                totalFPS += _fpsSamples[i];

            AverageFPS = (short)(totalFPS / _fpsSamplesCount);

            OnePercentFPS = _calculatePercentileFPS(_onePercentSamples);

            Zero1PercentFps = _calculatePercentileFPS(_zero1PercentSamples);
        }

        private void _removeFromSortedArray(short value)
        {
            if (_fpsSamplesCount <= 0)
            {
                return;
            }

            for (int i = 0; i < _fpsSamplesCount; i++)
            {
                if (_sortedSamples[i] == value)
                {
                    int remainingLength = _fpsSamplesCount - i - 1;

                    if (remainingLength > 0)
                    {
                        Array.Copy(_sortedSamples, i + 1, _sortedSamples, i, remainingLength);
                    }

                    _sortedSamples[_fpsSamplesCount - 1] = 0;
                    break;
                }
            }
        }

        private void _insertIntoSortedArray(short value)
        {
            if (_fpsSamplesCount == 0)
            {
                _sortedSamples[0] = value;
                return;
            }

            int insertIndex = 0;
            for (int i = 0; i < _fpsSamplesCount; i++)
            {
                insertIndex = i;
                if (_sortedSamples[i] > value)
                {
                    break;
                }
            }

            if (insertIndex >= FPS_SAMPLES_CAPACITY)
            {
                UnityEngine.Debug.Log('+');
                return;
            }

            int remainingLength = _fpsSamplesCount - insertIndex;

            if (remainingLength > 0 && insertIndex + remainingLength < FPS_SAMPLES_CAPACITY)
            {
                Array.Copy(_sortedSamples, insertIndex, _sortedSamples, insertIndex + 1, remainingLength);
            }

            _sortedSamples[insertIndex] = value;
        }
        
        private short _calculatePercentileFPS(short sampleCount)
        {
            if (_fpsSamplesCount == 0) return 0;

            sampleCount = (short)BMath.Min(sampleCount, _fpsSamplesCount);
            long total = 0;

            for (int i = 0; i < sampleCount; i++)
                total += _sortedSamples[i];

            return (short)(total / sampleCount);
        }

        private void _updateParameters()
        {
            _onePercentSamples = FPS_SAMPLES_CAPACITY / 100;
            _zero1PercentSamples = FPS_SAMPLES_CAPACITY / 1000;
        }
    }
}
