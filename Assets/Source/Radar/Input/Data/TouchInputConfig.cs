using System;
using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Radar.Input
{
    [Serializable]
    public class TouchInputConfig : IConfig
    {
        public float discreteZoomMultiplier;
        public float continuousZoomMultiplier;
        public bool useDiscreteZoom;
        
        public IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}