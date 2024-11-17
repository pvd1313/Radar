using System;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [Serializable]
    public struct TempRenderTextureData : IValidatable
    {
        public int antiAliasing;
        public RenderTextureReadWrite readWrite;
        public RenderTextureMemoryless memoryless;
        public VRTextureUsage vrUsage;
        
        public string Validate()
        {
            if (antiAliasing < 1)
            {
                return "Invalid antiAliasing: " + antiAliasing;
            }

            return null;
        }
    }
}