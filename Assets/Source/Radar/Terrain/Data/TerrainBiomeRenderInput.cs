using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainBiomeRenderInput
    {
        public int2 screenSize;
        public float2 renderPosition;
        public float viewDistance;
        public RenderTexture bakedTerrain;
    }
}