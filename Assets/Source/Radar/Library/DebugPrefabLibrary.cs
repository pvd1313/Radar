using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.UI;

namespace Beast.Radar
{
    public class DebugPrefabLibrary : ScriptableConfig
    {
        public DebugMenuView debugMenu;
        public DebugItemButton debugItemButton;
        public DebugItemTypeButton debugItemTypeButton;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}