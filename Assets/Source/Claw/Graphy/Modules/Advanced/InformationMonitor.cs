using System;
using System.Globalization;
using Beast.Claw.Common;
using Beast.Claw.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class InformationMonitor
    {
        public event Action WindowResolutionChanged;
        public AdvancedData Data { get; private set; }

        private Cache<int2> _windowResolutionCache;
        private StringBuilder _stringBuilder;

        private readonly string[] _windowStrings =
        {
            "Window: ",
            "x",
            "[",
            "dpi]"
        };

        public void Initialize()
        {
            _stringBuilder = new StringBuilder(256);

            Resolution displayResolution = Screen.currentResolution;

            _windowResolutionCache = new Cache<int2>();

            _windowResolutionCache.Update(new int2(Screen.width, Screen.height));

            Data = new AdvancedData
            {
                graphicsDeviceVersion = $"Graphics API: {SystemInfo.graphicsDeviceVersion}",
                processorType = $"CPU: {SystemInfo.processorType} [{SystemInfo.processorCount} cores]",
                systemMemory = $"RAM {SystemInfo.systemMemorySize} MB",
                graphicsDeviceName = $"GPU: {SystemInfo.graphicsDeviceName}",
                graphicsMemorySize = $"VRAM: {SystemInfo.graphicsMemorySize} " +
                                     $"MB. Max texture size: {SystemInfo.maxTextureSize} " +
                                     $"px. Shader level: {SystemInfo.graphicsShaderLevel}",
                screenResolution = $"Screen: {displayResolution.width}x{displayResolution.height} " +
                                   $"@{Screen.currentResolution.refreshRateRatio.value.ToString(CultureInfo.InvariantCulture)} Hz",
                gameWindowResolution = _getWindowResolution(),
                operatingSystem = $"OS: {SystemInfo.operatingSystem} [{SystemInfo.deviceType}]"
            };
        }

        public void Update()
        {
            if (_windowResolutionCache.Update(new int2(Screen.width, Screen.height)))
            {
                Data = Data.UpdateWindowResolution(_getWindowResolution());

                WindowResolutionChanged?.Invoke();
            }
        }

        private string _getWindowResolution()
        {
            _stringBuilder.Clear();

            _stringBuilder
                .Add(_windowStrings[0])
                .Add(Screen.width)
                .Add(_windowStrings[1])
                .Add(Screen.height)
                .Add(" ")
                .Add(_windowStrings[2])
                .Add((int)Screen.dpi)
                .Add(_windowStrings[3]);

            return _stringBuilder.ToString();
        }
    }
}