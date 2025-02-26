using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct TextureToRenderTextureData
    {
        public Texture sourceTexture;
        public int2 sourceOffset;
        public int2 sourceSize;
        public RenderTexture targetTexture;
        public int2 targetOffset;
    }
}