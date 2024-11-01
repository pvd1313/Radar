using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBiomeRender : IReady, IRender<TerrainBiomeRenderTask>
    {
        Texture Texture { get; }
        public float2 TexelSize { get; }
        float ViewDistance { get; }
        public float2 WorldToUVOffset(float2 worldPoint);
    }
}