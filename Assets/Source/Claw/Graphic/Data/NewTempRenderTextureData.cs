using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct NewTempRenderTextureData : IValidatable
    {
        public int2 size;
        public int depth;
        public RenderTextureFormat format;
        public RenderTextureReadWrite readWrite;
        public int antiAliasing;
        public RenderTextureMemoryless memoryless;
        public VRTextureUsage vrUsage;
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