using Beast.Radar.Terrain;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public interface IBiomeTexture
    {
        public int2 Size { get; }

        TerrainCell GetPixel(int2 position);

        int2 WorldPositionToCell(float2 position);
    }
}