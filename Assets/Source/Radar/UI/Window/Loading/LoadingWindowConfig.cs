using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.UI
{
    public class LoadingWindowConfig : ScriptableConfig
    {
        public XamlAsset asset;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}