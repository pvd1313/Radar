using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.UI.Player;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIConfig : ScriptableConfig
    {
        public XamlAsset HUDWindowAsset;
        public XamlAsset pauseWindowAsset;
        public XamlAsset mapGenerationWindowAsset;
        public XamlAsset graphyAsset;
        
        public PlayerViewData playerViewData;
        public TargetViewData targetViewData;
        
        public Sprite[] citySprites;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}