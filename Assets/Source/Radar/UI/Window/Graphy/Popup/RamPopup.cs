using Beast.Claw.Common;
using Beast.Claw.Graphy.Modules.Ram;
using Beast.Claw.UI;
using Beast.Radar.Player;
using Noesis;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class RamPopup : GraphyPopup
    {
        private readonly PlayerPreference _preference;
        private readonly RamMonitor _monitor;
        private readonly RamRender _render;
        private readonly GraphyRamAssets _assets;
        private readonly int _updateRate;
        
        private IntToStringConverter _converter;
        private float _deltaTime;
        
        public RamPopup(
            PlayerPreference preference,
            RamMonitor monitor,
            RamRender render,
            GraphyRamAssets assets,
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
            _converter = new IntToStringConverter(0, 16386, "D2");

            _assets.button.IsChecked = _preference.graphy.isRamEnabled;

            _assets.button.Click += (_, _) =>
            {
                _preference.graphy.isRamEnabled = !_preference.graphy.isRamEnabled;
                
                Debug.Log("click");
                RefreshState();
            };
            
            _assets.displayAllocated.SetMaterial(_render.GetAllocatedMaterial());
            _assets.displayReserved.SetMaterial(_render.GetReservedMaterial());
            _assets.displayMono.SetMaterial(_render.GetMonoMaterial());
        }

        public override void RefreshState()
        {
            _monitor.SetEnable(_preference.graphy.isRamEnabled);
            _render.SetEnable(_preference.graphy.isRamEnabled);
            
            _assets.view.Visibility = _preference.graphy.isRamEnabled 
                ? Visibility.Visible 
                : Visibility.Collapsed;
            
            _deltaTime = 0;
        }

        public override void Update()
        {
            if (!_preference.graphy.isRamEnabled)
            {
                return;
            }

            _deltaTime += Time.unscaledDeltaTime;

            if (_deltaTime > 1f / _updateRate)
            {
                _assets.allocated.Text = _converter.Convert((int)_monitor.AllocatedRam);
                _assets.reserved.Text = _converter.Convert((int)_monitor.ReservedRam);
                _assets.mono.Text = _converter.Convert((int)_monitor.MonoRam);

                _deltaTime = 0f;
            }
        }
    }
}