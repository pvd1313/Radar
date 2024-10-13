using Beast.Claw.Common;
using Unity.Mathematics;
using Zenject;

namespace Beast.Radar.Debug
{
    public class DebugController : IInitializable
    {
        private readonly DebugConfig _config;

        public DebugController(DebugConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
#if BEAST_DEVELOPMENT
            
            if (_config.enableLogConsole)
            {
                BScene.InstantiateGameObject(this, _config.logConsolePrefab, float3.zero, quaternion.identity);
            }

            if (_config.enableFPSCounter)
            {
                BScene.InstantiateGameObject(this, _config.fpsCounterPrefab, float3.zero, quaternion.identity);
            }
            
#endif
        }
    }
}