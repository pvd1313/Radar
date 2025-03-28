using System.Collections.Generic;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadRenderConfig : ScriptableConfig
    {
        public Shader renderShader;
        public Texture2D texture;
        public RoadRenderLODConfig[] lods;
        public AnimationCurve viewDistanceToPixelSizeCurve;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}