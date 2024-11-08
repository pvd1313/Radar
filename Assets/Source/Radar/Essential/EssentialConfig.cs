using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Resources;

namespace Beast.Radar.Essential
{
    [Serializable]
    public class EssentialConfig : IConfig
    {
        public List<Address<ScriptableInstaller>> coreInstallers; 
        public Address<ScriptableInstaller> debugInstaller; 
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}