using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class BeastDebugConfig : BeastConfig
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        public GameObject logConsolePrefab;
        public GameObject fpsCounterPrefab;

        public bool enableLogConsole;
        public bool enableFPSCounter;
        
#endif
    }
}