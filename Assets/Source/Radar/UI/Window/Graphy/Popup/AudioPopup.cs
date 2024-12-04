using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Beast.Claw.Graphy.Modules;
using Beast.Claw.Math;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class AudioPopup
    {
        private readonly TextBlock _textDB;
        private readonly AudioMonitor _audioMonitor;
        private readonly IntToStringConverter _converter;
        private readonly int _updateRate;
        
        private float _deltaTimeOffset;

        public AudioPopup(GraphyConfig config, AudioMonitor audioMonitor, TextBlock textBlock)
        {
            _updateRate = config.audioTextUpdateRate;
            _audioMonitor = audioMonitor;
            _textDB = textBlock;
            _converter = new IntToStringConverter(-80, 0, "D2");
        }
        
        public void Update()
        {
            if (_audioMonitor.SpectrumDataAvailable)
            {
                if (_deltaTimeOffset > 1f / _updateRate)
                {
                    _deltaTimeOffset = 0f;
                    
                    _textDB.Text = _converter.Convert((int)BMath.Clamp(_audioMonitor.MaxDB, -80, 0));
                }
                else
                {
                    _deltaTimeOffset += Time.deltaTime;
                }
            }
        }
    }
}