using Beast.Radar.Terrain;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public readonly struct BiomeTextureWithBlocks : IBiomeTexture
    {
        public int2 Size => _source.blockCountX * _source.blockSize;
        
        private readonly TextureWithBlocks<EBiomeName> _source;

        public BiomeTextureWithBlocks(TextureWithBlocks<EBiomeName> source)
        {
            _source = source;
        }
        
        public EBiomeName GetPixel(int2 position)
        {
            return _source.GetPixel(position);
        }

        public int2 WorldPositionToCell(float2 position)
        {
            return _source.WorldPositionToCell(position);
        }
    }
}