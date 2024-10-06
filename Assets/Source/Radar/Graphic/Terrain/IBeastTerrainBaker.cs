using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.World
{
    public interface IBeastTerrainBaker
    {
        public void BakeTexture(TerrainBakerInput input);
        
        RenderTexture CreateTexture<TOwner>(TOwner owner, int2 size, bool isTemporary);
    }
}