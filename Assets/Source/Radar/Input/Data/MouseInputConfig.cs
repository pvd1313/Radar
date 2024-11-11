using System;
using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Input
{
    [Serializable]
    public class MouseInputConfig : IConfig
    {
        public float zoomMultiplier;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}