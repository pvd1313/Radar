using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public struct TerrainBakerInput : IValidatable
    {
        public float biomeScale99;
        public float seed01;
        public float waterFlood01;
        public RenderTexture targetTexture;
        
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            
            if (targetTexture == null)
            {
                errorMessage = $"{nameof(targetTexture)} is null";
                return false;
            }

            return true;
        }
    }
}