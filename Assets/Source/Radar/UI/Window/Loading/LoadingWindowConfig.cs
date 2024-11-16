using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class LoadingWindowConfig : ScriptableConfig
    {
        public Texture2D backgroundTexture;
        public NoesisXaml view;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}