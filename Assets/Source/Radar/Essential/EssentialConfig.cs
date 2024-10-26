using System.Collections.Generic;
using Beast.Claw.Asset;
using Beast.Claw.Common;

namespace Beast.Radar.Essential
{
    public class EssentialConfig : ScriptableConfig
    {
        public List<AssetReference<ScriptableInstaller>> coreInstallers;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}