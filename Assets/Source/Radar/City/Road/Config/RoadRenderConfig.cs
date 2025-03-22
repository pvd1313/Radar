using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadRenderConfig : ScriptableConfig
    {
        public float lod2SegmentLength;
        public Shader renderShader;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}