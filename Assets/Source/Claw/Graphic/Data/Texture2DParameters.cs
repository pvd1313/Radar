using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [Serializable]
    public struct Texture2DParameters : IValidatable
    {
        public PlanarTextureData texture;

        public TextureFormat format;
        public bool mipChain;
        public bool isLinear;
        public bool createInitialized;
        
        public string Validate()
        {
            return texture.Validate();
        }

        public Texture2DParameters WithSize(int2 size)
        {
            Texture2DParameters data = this;
            
            data.texture.size = size;

            return data;
        }
    }
}