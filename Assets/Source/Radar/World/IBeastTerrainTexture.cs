using UnityEngine;

namespace Beast.Radar.World
{
    public interface IBeastTerrainTexture
    {
        public RenderTexture Texture { get; }
        
        public bool IsReady { get; }
    }
}