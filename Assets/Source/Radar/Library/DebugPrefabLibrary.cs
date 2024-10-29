using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Resources;
using Beast.Radar.UI;
using UnityEngine;

namespace Beast.Radar
{
    public class DebugPrefabLibrary : ScriptableConfig
    {
        public DebugMenuView debugMenu;
        public DebugItemButton debugItemButton;
        public DebugItemTypeButton debugItemTypeButton;
        public InputIconDisplay inputIconDisplay;

        public Address<GameObject> logConsole;
        public Address<GameObject> fpsCounter;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}