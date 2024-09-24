using Unity.Mathematics;
using UnityEngine.Experimental.Rendering;

namespace Beast.Claw.CG
{
    public struct RenderTextureParameters
    {
        public int2 size;
        public int depth;
        public GraphicsFormat format;
        public int mipCount;
        public string name;
    }
}