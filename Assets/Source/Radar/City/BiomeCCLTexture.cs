using Beast.Claw.Math.CCL;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public readonly struct BiomeCCLTexture : ICCLTexture
    {
        public int2 Size => _textureSize;

        private readonly NativeArray<EBiomeName> _texture;
        private readonly int2 _textureSize;

        public BiomeCCLTexture(NativeArray<EBiomeName> sourceTexture, int2 sourceTextureSize)
        {
            _texture = sourceTexture;
            _textureSize = sourceTextureSize;
        }
        
        public bool TestPixel(int x, int y)
        {
            return _texture[y * _textureSize.x + x] != EBiomeName.Deep;
        }
    }
}