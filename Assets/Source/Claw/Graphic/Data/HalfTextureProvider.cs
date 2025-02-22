using System.Collections.Generic;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct HalfTextureProvider
    {
        private readonly Dictionary<int2, Texture2D> _pool;
        private readonly string _textureName;
        private readonly IController _textureOwner;
        
        public HalfTextureProvider(IController owner, string textureName)
        {
            _pool = new Dictionary<int2, Texture2D>();
            _textureOwner = owner;
            _textureName = textureName;
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
                format = TextureFormat.RGBAHalf,
                isLinear = true,
                mipChain = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = _textureName,
                size = textureSize
            });

            _pool[textureSize] = readyTexture;

            return readyTexture;
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