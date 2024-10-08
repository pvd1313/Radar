using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainBaker
    {
        void Bake(TerrainData terrain, RenderTexture targetTexture);
        
        RenderTexture Create<TOwner>(TOwner owner, int2 size, bool isTemporary);
    }
}