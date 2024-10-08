using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct BlitParameters
    {
        public Texture sourceTexture;
        public RenderTexture targetTexture;
        public Material material;
        public int shaderPass;
    }
}