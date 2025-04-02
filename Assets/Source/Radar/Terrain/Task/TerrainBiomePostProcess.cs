using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainBiomePostProcess
    {
        public RenderTexture biomeTexture;
        public float renderDistance;
        public float2 renderPosition;
    }
}