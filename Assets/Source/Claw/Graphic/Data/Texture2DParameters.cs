using System.Drawing.Drawing2D;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct Texture2DParameters : IValidatable
    {
        public int2 size;
        public TextureFormat format;
        public bool mipChain;
        public bool isLinear;
        public string name;
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        public bool createInitialized;
        
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