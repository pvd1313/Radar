using Beast.Claw;
using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainFbmRenderTask : IValidatable
    {
        public TerrainData terrain;
        public float quality01;
        public RenderTexture outputTexture;
        
        public string Validate()
        {
            if (outputTexture == null)
            {
                return "outputTexture is null";
            }

            return terrain.Validate();
        }
    }
}