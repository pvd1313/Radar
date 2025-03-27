using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainBiomePostProcess
    {
        public RenderTexture biomeTexture;
        public float viewDistance;
        public float2 renderPosition;
    }
}