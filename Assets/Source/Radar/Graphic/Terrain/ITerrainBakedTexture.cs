using UnityEngine;

namespace Beast.Radar.Graphic
{
    public interface ITerrainBakedTexture
    {
        public RenderTexture Texture { get; }
        
        public bool IsReady { get; }
    }
}