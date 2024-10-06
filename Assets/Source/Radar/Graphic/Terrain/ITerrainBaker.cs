using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public interface ITerrainBaker
    {
        public void BakeTexture(TerrainBakerInput input);
        
        RenderTexture CreateTexture<TOwner>(TOwner owner, int2 size, bool isTemporary);
    }
}