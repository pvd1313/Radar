using Beast.Claw;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainBakeTask : IValidatable
    {
        public TerrainData terrain;
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