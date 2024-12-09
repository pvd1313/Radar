using Beast.Radar.Terrain;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public readonly struct BiomeTextureWithBlocks : IBiomeTexture
    {
        public int2 Size => _source.blockCountX * _source.blockSize;
        
        private readonly TextureWithBlocks<TerrainCell> _source;

        public BiomeTextureWithBlocks(TextureWithBlocks<TerrainCell> source)
        {
            _source = source;
        }
        
        public TerrainCell GetPixel(int2 position)
        {
            return _source.GetPixel(position);
        }

        public int2 WorldPositionToCell(float2 position)
        {
            return _source.WorldPositionToCell(position);
        }
    }
}