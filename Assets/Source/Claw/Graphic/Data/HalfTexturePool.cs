using System;
using Beast.Claw.Common;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct HalfTexturePool
    {
        private readonly Texture2D[] _pool;
        private readonly int _maxTextureSize;
        
        public HalfTexturePool(IController owner, int maxTextureSize)
        {
            Texture2DParameters textureData = new()
            {
                createInitialized = false,
                format = TextureFormat.RGBAHalf,
                isLinear = true,
                mipChain = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            _maxTextureSize = maxTextureSize;
            int maxTexturePower = BMath.IntCeilLog2(_maxTextureSize);
            
            _pool = new Texture2D[maxTexturePower + 1];
            
            for (int i = 0; i <= maxTexturePower; i++)
            {
                _pool[i] = BGraphic.CreateTexture2D(owner, textureData.WithSize(1 << i));
            }
        }

        public Texture2D GetTexture(int textureSize)
        {
            if (textureSize > _maxTextureSize)
            {
                throw new InvalidOperationException("Too big texture size");
            }

            return _pool[BMath.IntCeilLog2(textureSize)];
        }

        public void Dispose()
        {
            foreach (Texture2D texture in _pool)
            {
                BGraphic.Dispose(texture);
            }
        }
    }
}