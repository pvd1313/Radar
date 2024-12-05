using Beast.Claw.Graphy.Modules;
using Beast.Radar.Player;
using Noesis;

namespace Beast.Radar.UI
{
    public class InformationPopup : GraphyPopup
    {
        private readonly PlayerPreference _preference;
        private readonly GraphyInformationAssets _assets;
        private readonly InformationMonitor _monitor;

        public InformationPopup(PlayerPreference preference, GraphyInformationAssets assets, InformationMonitor monitor)
        {
            _preference = preference;
            _assets = assets;
            _monitor = monitor;
        }

        public override void Initialize()
        {
            _assets.button.IsChecked = _preference.graphy.isInformationEnabled;
            
            _monitor.WindowResolutionChanged += () =>
            {
                _assets.gameWindowResolutionText.Text = _monitor.Data.gameWindowResolution;
            };
            
            _assets.button.Click += (_, _) =>
            {
                _preference.graphy.isInformationEnabled = !_preference.graphy.isInformationEnabled;
                RefreshState();
            };
        }

        public override void RefreshState()
        {
            _assets.view.Visibility = _preference.graphy.isInformationEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (_preference.graphy.isInformationEnabled)
            {
                _assets.graphicsDeviceVersionText.Text = _monitor.Data.graphicsDeviceVersion;
                _assets.processorTypeText.Text = _monitor.Data.processorType;
                _assets.systemMemoryText.Text = _monitor.Data.systemMemory;
                _assets.graphicsDeviceNameText.Text = _monitor.Data.graphicsDeviceName;
                _assets.graphicsMemorySizeText.Text = _monitor.Data.graphicsMemorySize;
                _assets.screenResolutionText.Text = _monitor.Data.screenResolution;
                _assets.gameWindowResolutionText.Text = _monitor.Data.gameWindowResolution;
                _assets.operatingSystemText.Text = _monitor.Data.operatingSystem;
            }
        }
    }
}