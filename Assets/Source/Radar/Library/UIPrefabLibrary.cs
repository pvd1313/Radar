using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar
{
    [Serializable]
    public class UIPrefabLibrary : IConfig
    {
        public CanvasView canvas;
        
        public HUDView hud;
        public MapGenerationView mapGeneration;
        public PauseMenuView pauseMenu;
        public LoadingScreenView loadingScreen;
        public LoadingTaskText loadingTaskText;
        public DebugMenuView debugMenu;
        public DebugItemButton debugItemButton;
        public DebugItemTypeButton debugItemTypeButton;
        public InputIconDisplay inputIconDisplay;

        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}