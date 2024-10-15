using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainIndexRender : IReady
    {
        Texture Texture { get; }
        
        public float2 TexelSize { get; }
        
        float ViewDistance { get; }
        
        float RenderDistance { get; }

        public float2 WorldToUVOffset(float2 worldPoint);
    }
}