using System;
using Beast.Claw.Common;
using Beast.Claw.Text;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphy.Modules
{
    public class AdvancedMonitor
    {
        public event Action WindowResolutionChanged; 
        public AdvancedData Data { get; private set; }

        private Cache<int2> _windowResolutionCache;
        private StringBuilder _stringBuilder;

        private readonly string[] _windowStrings =
        {
            "Window: ",
            "x",
            "@",
            "Hz",
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
                                   $"@{Screen.currentResolution.refreshRateRatio.value} Hz",
                gameWindowResolution = _getWindowResolution(),
                operatingSystem = $"OS: {SystemInfo.operatingSystem} [{SystemInfo.deviceType}]"
            };
        }

        public void Update()
        {
            if (_windowResolutionCache.Update(new int2(Screen.width, Screen.height)))
            {
                Data.UpdateWindowResolution(_getWindowResolution());
                
                WindowResolutionChanged?.Invoke();
            }
        }
        
        private string _getWindowResolution()
        {
            _stringBuilder.Clear();

            _stringBuilder
                .Add(_windowStrings[0])
                .Add(_windowResolutionCache.Value.x)
                .Add(_windowStrings[1])
                .Add(_windowResolutionCache.Value.y)
                .Add(_windowStrings[2])
                .Add((int)Screen.currentResolution.refreshRateRatio.value)
                .Add(_windowStrings[3])
                .Add(_windowStrings[4])
                .Add((int)Screen.dpi)
                .Add(_windowStrings[5]);

            return _stringBuilder.ToString();
        }
    }
}