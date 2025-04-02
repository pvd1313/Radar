using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public readonly struct HalfTextureProvider
    {
        private readonly Dictionary<int2, Texture2D> _pool;
        private readonly string _textureName;
        private readonly IController _textureOwner;
        private readonly TextureFormat _textureFormat;
        
        public HalfTextureProvider(IController owner, string textureName, int halfPerPixel)
        {
            _pool = new Dictionary<int2, Texture2D>();
            _textureOwner = owner;
            _textureName = textureName;
            _textureFormat = _getTextureFormat(halfPerPixel);
        }

        public Texture2D GetTexture(int2 textureSize)
        {
            if (_pool.TryGetValue(textureSize, out Texture2D readyTexture))
            {
                return readyTexture;
            }

            readyTexture = BGraphic.CreateTexture2D(_textureOwner, new Texture2DParameters
            {
                createInitialized = false,
                format = _textureFormat,
                isLinear = true,
                mipChain = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = _textureName + "(" + textureSize.x + "," + + textureSize.y + ")",
                size = textureSize
            });

            _pool[textureSize] = readyTexture;

            return readyTexture;
        }

        private static TextureFormat _getTextureFormat(int pixelCount)
        {
            switch (pixelCount)
            {
                case 1: return TextureFormat.RHalf;
                case 2: return TextureFormat.RGHalf;
                case 4: return TextureFormat.RGBAHalf;
                
                default: throw new ArgumentException($"Invalid pixel count for texture format: {pixelCount}");
            }
        }

        public void Dispose()
        {
            foreach (Texture2D texture in _pool.Values)
            {
                BGraphic.Dispose(texture);
            }
            
            _pool.Clear();
        }
    }
}