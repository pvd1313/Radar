using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIConfig : ScriptableConfig
    {
        public XamlAsset HUDWindowAsset;
        public XamlAsset PauseWindowAsset;
        
        public Sprite[] citySprites;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}