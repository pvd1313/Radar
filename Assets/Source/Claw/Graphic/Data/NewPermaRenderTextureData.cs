using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct NewPermaRenderTextureData : IValidatable
    {
        public int2 size;
        public int depth;
        public RenderTextureFormat format;
        public int mipCount;
        public string name;
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        
        public string Validate()
        {
            if (size.x <= 0 || size.y <= 0)
            {
                return "Invalid size " + size;
            }

            return null;
        }
    }
}