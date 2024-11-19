using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.World;

namespace Beast.Radar.Serialization
{
    public class IntegratedSave : ScriptableConfig
    {
        public GameModel gameModel;
        public WorldData worldData;
        public string worldGuid;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}