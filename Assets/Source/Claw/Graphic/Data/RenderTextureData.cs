using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [Serializable]
    public struct RenderTextureData : IValidatable
    {
        public string name;
        public int2 size;
        public RenderTextureFormat format;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
        public int depth;
        public int mipCount;
        public int antiAliasing;
        public RenderTextureReadWrite readWrite;
        public RenderTextureMemoryless memoryless;
        public VRTextureUsage vrUsage;
        
        public string Validate()
        {
            if (size.x <= 0 || size.y <= 0)
            {
                return "Invalid size: " + size;
            }
            
            if (depth < 0)
            {
                return "Invalid depth: " + depth;
            }
            
            if (antiAliasing < 1)
            {
                return "Invalid antiAliasing: " + antiAliasing;
            }

            return null;
        }

        public static RenderTextureData Default()
        {
            return new RenderTextureData
            {
                antiAliasing = 1,
            };
        }

        public RenderTextureData WithSize(int2 size)
        {
            RenderTextureData data = this;
            
            data.size = size;

            return data;
        }

        public RenderTextureData WithFormat(RenderTextureFormat format)
        {
            RenderTextureData data = this;

            data.format = format;

            return data;
        }

        public RenderTextureData WithSize(Texture getSizeFromTexture)
        {
            getSizeFromTexture.ValidateNotNullThrowException(nameof(getSizeFromTexture));

            RenderTextureData data = this;

            data.size = getSizeFromTexture.Size();

            return data;
        }
    }
}