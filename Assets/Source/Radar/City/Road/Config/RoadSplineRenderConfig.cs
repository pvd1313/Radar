using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadSplineRenderConfig : ScriptableConfig
    {
        public Shader renderShader;
        public Texture2D texture;
        public RoadRenderLODConfig[] lods;
        public AnimationCurve viewDistanceToPixelSizeCurve;
        public FloatMinMax alphaFadeTilePixelSize;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}