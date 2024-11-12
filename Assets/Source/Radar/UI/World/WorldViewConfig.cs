using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class WorldViewConfig : ScriptableConfig
    {
        [Range(0,1)] public float[] cityVisibilityZoomLevels;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}