using UnityEngine;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBaker
    {
        void Bake(TerrainData terrain, RenderTexture targetTexture);
    }
}