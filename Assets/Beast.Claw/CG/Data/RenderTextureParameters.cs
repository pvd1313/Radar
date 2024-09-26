using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.CG
{
    public struct RenderTextureParameters
    {
        public int2 size;
        public int depth;
        public RenderTextureFormat format;
        public int mipCount;
        public string name;
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
    }
}