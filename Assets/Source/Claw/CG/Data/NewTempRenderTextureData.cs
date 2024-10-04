using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.CG
{
    public struct NewTempRenderTextureData : IBeastValidatable
    {
        public int2 size;
        public int depth;
        public RenderTextureFormat format;
        public RenderTextureReadWrite readWrite;
        public int antiAliasing;
        public RenderTextureMemoryless memoryless;
        public VRTextureUsage vrUsage;
        public FilterMode filterMode;
        
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            
            if (size.x <= 0 || size.y <= 0)
            {
                errorMessage = "Invalid size " + size;
                return false;
            }

            return true;
        }
    }
}