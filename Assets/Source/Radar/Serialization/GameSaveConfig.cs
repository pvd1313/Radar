using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Serialization
{
    public class GameSaveConfig : ScriptableConfig
    {
        public GameModel defaultSave;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}