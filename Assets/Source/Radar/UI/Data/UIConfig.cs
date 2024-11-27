using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Resources;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIConfig : ScriptableConfig
    {
        public XamlAsset HUDWindowAsset;
        public XamlAsset pauseWindowAsset;
        public XamlAsset mapGenerationWindowAsset;
        public XamlAsset graphyAsset;
    
        public Address<GameObject> graphy;

        public Sprite[] citySprites;
        
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}