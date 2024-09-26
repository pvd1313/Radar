using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.CG
{
    public struct NewPersistentRenderTextureData : IValidatable
    {
        public int2 size;
        public int depth;
        public RenderTextureFormat format;
        public int mipCount;
        public string name;
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        
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