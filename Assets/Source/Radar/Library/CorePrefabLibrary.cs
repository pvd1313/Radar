using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Player;
using Beast.Radar.UI;
using UnityEngine;

namespace Beast.Radar
{
    public class CorePrefabLibrary : ScriptableConfig
    {
        public PlayerView playerView;
        
        [Header("UI")]
        public HUDView hud;
        public MapGenerationView mapGeneration;
        public PauseMenuView pauseMenu;

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}