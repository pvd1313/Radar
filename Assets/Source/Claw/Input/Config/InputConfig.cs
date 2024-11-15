using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.Input
{
    public class InputConfig : ScriptableConfig
    {
        public TouchScreenConfig touchScreen;
        public MouseConfig mouse;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield return touchScreen;
            yield return mouse;
        }
    }
}