using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct TextureCopyParameters
    {
        public Texture sourceTexture;
        public int2 sourceOffset;
        public int2 sourceSize;
        public int sourceMip;
        public int sourceElement;
        
        public Texture targetTexture;
        public int2 targetOffset;
        public int targetElement;
        public int targetMip;
    }
}