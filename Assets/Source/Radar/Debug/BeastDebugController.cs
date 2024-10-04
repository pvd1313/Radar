using Beast.Claw.Common;
using Unity.Mathematics;
using Zenject;

namespace Beast.Radar.Debug
{
    public class BeastDebugController : IInitializable
    {
        private readonly BeastDebugConfig _config;

        public BeastDebugController(BeastDebugConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            
            if (_config.enableLogConsole)
            {
                BeastScene.InstantiateGameObject(this, _config.logConsolePrefab, float3.zero, quaternion.identity);
            }

            if (_config.enableFPSCounter)
            {
                BeastScene.InstantiateGameObject(this, _config.fpsCounterPrefab, float3.zero, quaternion.identity);
            }
#endif
        }
    }
}