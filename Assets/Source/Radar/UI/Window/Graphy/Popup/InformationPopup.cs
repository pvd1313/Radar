using Beast.Claw.Graphy.Modules;
using Beast.Radar.Player;
using Google.GData.Documents;
using Noesis;

namespace Beast.Radar.UI
{
    public class InformationPopup
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

        public void Initialize()
        {
            _monitor.WindowResolutionChanged += OnWindowResolutionChanged;
            
            _assets.button.Click += (_, _) =>
            {
                _preference.graphy.isInformationEnabled = !_preference.graphy.isInformationEnabled;
                SetState();
            };
            
            _assets.graphicsDeviceVersionText.Text = _monitor.Data.graphicsDeviceVersion;
            _assets.processorTypeText.Text = _monitor.Data.processorType;
            _assets.systemMemoryText.Text = _monitor.Data.systemMemory;
            _assets.graphicsDeviceNameText.Text = _monitor.Data.graphicsDeviceName;
            _assets.graphicsMemorySizeText.Text = _monitor.Data.graphicsMemorySize;
            _assets.screenResolutionText.Text = _monitor.Data.screenResolution;
            _assets.gameWindowResolutionText.Text = _monitor.Data.gameWindowResolution;
            _assets.operatingSystemText.Text = _monitor.Data.operatingSystem;
            
            
            
            
            SetState();
        }

        private void SetState()
        {
            _assets.view.Visibility = _preference.graphy.isInformationEnabled
                ? Visibility.Visible
                : Visibility.Collapsed;
            
            
        }

        private void OnWindowResolutionChanged()
        {
            _assets.gameWindowResolutionText.Text = _monitor.Data.gameWindowResolution;
        }

        /*
        public void SetState(EModuleState state)
        {
            _currentState = state;

            bool active = state == EModuleState.FULL
                          || state == EModuleState.TEXT
                          || state == EModuleState.BASIC;

            gameObject.SetActive(active);

            m_backgroundImages.SetAllActive(active && m_graphyManager.Background);
        }*/
    }
}