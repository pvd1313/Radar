using System;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [Serializable]
    public struct PlanarTextureData : IValidatable
    {
        public string name;
        public int2 size;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
        
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