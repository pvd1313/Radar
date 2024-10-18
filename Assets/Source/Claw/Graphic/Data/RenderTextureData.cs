using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [Serializable]
    public struct RenderTextureData : IValidatable
    {
        public PlanarTextureData texture;
        
        public RenderTextureFormat format;
        public int depth;
        
        public int mipCount;

        public TempRenderTextureData temp;
        
        public string Validate()
        {
            if (depth < 0)
            {
                return "Invalid depth: " + depth;
            }

            string result = temp.Validate();

            if (result != null)
            {
                return result;
            }
            
            return texture.Validate();
        }

        public RenderTextureData WithSize(int2 size)
        {
            RenderTextureData data = this;
            
            data.texture.size = size;

            return data;
        }

        public RenderTextureData WithSize(Texture getSizeFromTexture)
        {
            getSizeFromTexture.ValidateNotNullThrowException(nameof(getSizeFromTexture));

            RenderTextureData data = this;

            data.texture.size = getSizeFromTexture.Size();

            return data;
        }
    }
}