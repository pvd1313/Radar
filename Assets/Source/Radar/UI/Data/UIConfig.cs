using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.UI.Player;

namespace Beast.Radar.UI
{
    public class UIConfig : ScriptableConfig
    {
        public XamlAsset HUDWindowAsset;
        public XamlAsset pauseWindowAsset;
        public XamlAsset mapGenerationWindowAsset;
        public XamlAsset graphyAsset;
        public XamlAsset mainMenuAsset;
        
        public PlayerViewData playerViewData;
        public TargetViewData targetViewData;

        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}