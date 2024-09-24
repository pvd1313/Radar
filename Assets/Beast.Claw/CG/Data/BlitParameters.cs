using UnityEngine;

namespace Beast.Claw.CG
{
    public struct BlitParameters
    {
        public Texture sourceTexture;
        public RenderTexture targetTexture;
        public Material material;
        public int shaderPass;
    }
}