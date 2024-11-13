using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class LoadingScreenConfig : ScriptableConfig
    {
        public Texture backgroundTexture;
        public NoesisXaml loadingScreenView;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}