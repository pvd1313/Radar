using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.City
{
    public class RoadRenderConfig : ScriptableConfig
    {
        public float lod2SegmentLength;
        public float lod1CellSize;
        public float lod1SegmentLength;
        public float lod1ActivationDistance;
        public Shader renderShader;
        public Texture2D texture;
        public FloatMinMax roadPixelSize;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
    }
}