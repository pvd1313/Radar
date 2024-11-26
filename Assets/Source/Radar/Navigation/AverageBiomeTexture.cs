using Beast.Radar.Terrain;
using Unity.Mathematics;

namespace Beast.Radar.Navigation
{
    public readonly struct AverageBiomeTexture : IBiomeTexture
    {
        public int2 Size => _source.cellCountX;

        private readonly AveragePixelCountTexture<EBiomeName> _source;

        public AverageBiomeTexture(AveragePixelCountTexture<EBiomeName> source)
        {
            _source = source;
        }
        
        public EBiomeName GetPixel(int2 position)
        {
            return _source.texture[position.y * _source.cellCountX + position.x];
        }

        public int2 WorldPositionToCell(float2 position)
        {
            return _source.WorldPositionToCell(position);
        }
    }
}