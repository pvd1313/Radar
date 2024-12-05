using Beast.Claw.Common;
using Beast.Claw.Graphy;
using Beast.Claw.Graphy.Modules;
using Beast.Claw.Math;
using Beast.Claw.UI;
using Beast.Radar.Player;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class AudioPopup : GraphyPopup
    {
        private readonly PlayerPreference _preference;
        private readonly AudioMonitor _monitor;
        private readonly AudioRender _render;
        private readonly GraphyAudioAssets _assets;
        private readonly int _updateRate;
        
        private IntToStringConverter _converter;
        private float _deltaTimeOffset;

        public AudioPopup(
            PlayerPreference preference, 
            AudioMonitor monitor,
            AudioRender render,
            GraphyAudioAssets assets,
            int updateRate)
        {
            _preference = preference;
            _monitor = monitor;
            _render = render;
            _assets = assets;
            _updateRate = updateRate;
        }

        public override void Initialize()
        {
            _converter = new IntToStringConverter(-80, 0, "D2");
            
            _assets.displayCurrent.SetMaterial(_render.GetCurrentMaterial());
            
            _assets.displayPrevious.SetMaterial(_render.getPreviousMaterial());
            
            _assets.button.IsChecked = _preference.graphy.isAudioEnabled;
            
            _assets.button.Click += (_, _) =>
            {
                _preference.graphy.isAudioEnabled = !_preference.graphy.isAudioEnabled;
            
                RefreshState();
            };
        }
        
        public override void Update()
        {
            if (!_preference.graphy.isAudioEnabled)
            {
                return;
            }
            
            if (_monitor.SpectrumDataAvailable)
            {
                if (_deltaTimeOffset > 1f / _updateRate)
                {
                    _deltaTimeOffset = 0f;
                    
                    _assets.db.Text = _converter.Convert((int)BMath.Clamp(_monitor.MaxDB, -80, 0));
                }
                else
                {
                    _deltaTimeOffset += Time.deltaTime;
                }
            }
        }

        public override void RefreshState()
        {
            _monitor.SetEnable(_preference.graphy.isAudioEnabled);
         
            _render.SetEnable(_preference.graphy.isAudioEnabled);

            _assets.view.Visibility = _preference.graphy.isAudioEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}