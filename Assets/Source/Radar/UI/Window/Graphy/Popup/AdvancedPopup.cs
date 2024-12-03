using Beast.Claw.Graphy.Modules;

namespace Beast.Radar.UI
{
    public class AdvancedPopup
    {
        private readonly GraphyAdvancedAssets _assets;
        private readonly AdvancedMonitor _monitor;

        public AdvancedPopup(GraphyAdvancedAssets assets, AdvancedMonitor monitor)
        {
            _assets = assets;
            _monitor = monitor;
        }

        public void Initialize()
        {
            _monitor.WindowResolutionChanged += OnWindowResolutionChanged;

            _assets.graphicsDeviceVersionText.Text = _monitor.Data.graphicsDeviceVersion;
            _assets.processorTypeText.Text = _monitor.Data.processorType;
            _assets.systemMemoryText.Text = _monitor.Data.systemMemory;
            _assets.graphicsDeviceNameText.Text = _monitor.Data.graphicsDeviceName;
            _assets.graphicsMemorySizeText.Text = _monitor.Data.graphicsMemorySize;
            _assets.screenResolutionText.Text = _monitor.Data.screenResolution;
            _assets.gameWindowResolutionText.Text = _monitor.Data.gameWindowResolution;
            _assets.operatingSystemText.Text = _monitor.Data.operatingSystem;
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