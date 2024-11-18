using System;
using Beast.Claw.Common;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    [Serializable]
    public struct Texture2DParameters : IValidatable
    {
        public string name;
        public int2 size;
        public TextureFormat format;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
        public bool mipChain;
        public bool isLinear;
        public bool createInitialized;
        
        public string Validate()
        {
            if (size.x <= 0 || size.y <= 0)
            {
                return "Invalid size " + size;
            }

            return null;
        }

        public Texture2DParameters WithSize(int2 size)
        {
            Texture2DParameters data = this;
            
            data.size = size;

            return data;
        }
    }
}