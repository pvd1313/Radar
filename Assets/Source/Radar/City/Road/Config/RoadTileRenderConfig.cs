using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadTileRenderConfig : ScriptableConfig
    {
        public float cellSize;
        public Shader pixelPlotShader;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}