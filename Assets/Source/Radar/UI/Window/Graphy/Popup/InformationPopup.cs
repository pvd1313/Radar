using System.Runtime.CompilerServices;
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
                if (_preference.graphy.isInformationEnabled)
                {
                    // Graphics Device Version
                    _assets.graphicsDeviceVersionText.Inlines.Clear();
                    _assets.graphicsDeviceVersionText.Inlines.Add(_asRun("GAPI: ", _assets.headerTextBrush));
                    _assets.graphicsDeviceVersionText.Inlines.Add(_asRun(_monitor.Data.graphicsDeviceVersion,
                        _assets.defaultTextBrush));

                    // Processor Type
                    _assets.processorTypeText.Inlines.Clear();
                    _assets.processorTypeText.Inlines.Add(_asRun("CPU: ", _assets.headerTextBrush));
                    _assets.processorTypeText.Inlines.Add(_asRun(_monitor.Data.processorType,
                        _assets.defaultTextBrush));

                    // System Memory
                    _assets.systemMemoryText.Inlines.Clear();
                    _assets.systemMemoryText.Inlines.Add(_asRun("RAM: ", _assets.headerTextBrush));
                    _assets.systemMemoryText.Inlines.Add(_asRun(_monitor.Data.systemMemory, _assets.defaultTextBrush));

                    // Graphics Device Name
                    _assets.graphicsDeviceNameText.Inlines.Clear();
                    _assets.graphicsDeviceNameText.Inlines.Add(_asRun("GPU: ", _assets.headerTextBrush));
                    _assets.graphicsDeviceNameText.Inlines.Add(_asRun(_monitor.Data.graphicsDeviceName,
                        _assets.defaultTextBrush));

                    // Graphics Memory Size
                    _assets.graphicsMemorySizeText.Inlines.Clear();
                    _assets.graphicsMemorySizeText.Inlines.Add(_asRun("VRAM: ", _assets.headerTextBrush));
                    _assets.graphicsMemorySizeText.Inlines.Add(_asRun(_monitor.Data.graphicsMemorySize,
                        _assets.defaultTextBrush));

                    // Screen Resolution
                    _assets.screenResolutionText.Inlines.Clear();
                    _assets.screenResolutionText.Inlines.Add(_asRun("Screen: ", _assets.headerTextBrush));
                    _assets.screenResolutionText.Inlines.Add(_asRun(_monitor.Data.screenResolution,
                        _assets.defaultTextBrush));

                    // Game Window Resolution
                    _assets.gameWindowResolutionText.Inlines.Clear();
                    _assets.gameWindowResolutionText.Inlines.Add(_asRun("Window: ", _assets.headerTextBrush));
                    _assets.gameWindowResolutionText.Inlines.Add(_asRun(_monitor.Data.gameWindowResolution,
                        _assets.defaultTextBrush));

                    // Operating System
                    _assets.operatingSystemText.Inlines.Clear();
                    _assets.operatingSystemText.Inlines.Add(_asRun("OS: ", _assets.headerTextBrush));
                    _assets.operatingSystemText.Inlines.Add(_asRun(_monitor.Data.operatingSystem,
                        _assets.defaultTextBrush));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Run _asRun(string text, Brush brush)
        {
            return new Run(text) { Foreground = brush };
        }
    }
}