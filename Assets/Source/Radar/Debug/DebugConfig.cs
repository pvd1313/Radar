using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugConfig : Config
    {
#if BEAST_DEVELOPMENT
        
        public GameObject logConsolePrefab;
        public GameObject fpsCounterPrefab;

        public bool enableLogConsole;
        public bool enableFPSCounter;
        
#endif
    }
}