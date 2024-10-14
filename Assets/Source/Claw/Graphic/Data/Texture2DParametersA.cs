using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct Texture2DParametersA : IValidatable
    {
        public RenderTexture sourceTexture;
        public TextureFormat format;
        public bool mipChain;
        public bool isLinear;
        public string name;
        public FilterMode filterMode;

        public string Validate()
        {
            if (sourceTexture == null)
            {
                return "sourceTexture is null";
            }

            return null;
        }
    }
}