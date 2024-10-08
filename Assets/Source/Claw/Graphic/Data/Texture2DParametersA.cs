using Beast.Claw.Common;
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

        public bool IsValid(out string message)
        {
            message = null;

            if (sourceTexture == null)
            {
                message = "sourceTexture is null";
                return false;
            }

            return true;
        }
    }
}