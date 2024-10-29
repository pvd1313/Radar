using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Resources;
using Beast.Radar.InputHandling;
using Beast.Radar.Player;
using Beast.Radar.UI;
using UnityEngine;

namespace Beast.Radar
{
    public class CorePrefabLibrary : ScriptableConfig
    {
        public PlayerInputView playerInput;
        public PlayerView playerView;
        
        [Header("UI")]
        public HUDView hud;
        public MapGenerationView mapGeneration;
        public PauseMenuView pauseMenu;

        public Address<DebugPrefabLibrary> debug;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}