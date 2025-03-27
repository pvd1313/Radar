using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    public struct TerrainBiomeRenderResult
    {
        public float2 renderPosition;
        public float viewDistance;
        public float textureYScale;
        public float cellPixelSize;
        public float snapGridSize;
        public float renderDistance;
        public float texelSize;
        public float noiseSamplerLOD;
    }
}