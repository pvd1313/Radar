using Beast.Claw;

namespace Beast.Radar.Terrain
{
    public struct TerrainPreviewRenderTask : IValidatable
    {
        public TerrainData terrain;
        public TerrainPreviewTexture texture;
        
        public string Validate()
        {
            if (!texture.IsValid(out string error))
            {
                return error;
            }

            return terrain.Validate();
        }
    }
}