using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainDetailRender : IReady
    {
        Texture Texture { get; }
        
        public float2 TexelSize { get; }
        
        public float2 WorldToUVOffset(float2 worldPoint);
    }
}