using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct Texture2DParameters : IValidatable
    {
        public PlanarTextureData texture;

        public TextureFormat format;
        public bool mipChain;
        public bool isLinear;
        public bool createInitialized;
        
        public string Validate()
        {
            return texture.Validate();
        }
    }
}