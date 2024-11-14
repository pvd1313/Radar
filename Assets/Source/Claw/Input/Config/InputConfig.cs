using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public class InputConfig : ScriptableConfig
    {
        public TouchInputConfig touch;
        public MouseInputConfig mouse;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return touch;
            yield return mouse;
        }
    }
}