using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Serialization
{
    public class IntegratedSave : ScriptableConfig
    {
        public GameModel model;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}