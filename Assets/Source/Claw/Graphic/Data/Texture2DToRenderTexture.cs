using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct Texture2DToRenderTexture : IValidatable
    {
        public RenderTexture targetTexture;
        public Texture2D sourceTexture;
        public float2 targetTexturePixelOffset;
        public string Validate()
        {
            if (targetTexture == null)
            {
                return $"{nameof(targetTexture)} is null";
            }

            if (sourceTexture == null)
            {
                return $"{nameof(sourceTexture)} is null";
            }
            
            return null;
        }
    }
}