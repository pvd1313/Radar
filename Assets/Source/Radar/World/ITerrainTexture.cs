using UnityEngine;

namespace Beast.Radar.World
{
    public interface ITerrainTexture
    {
        public RenderTexture Texture { get; }
        
        public bool IsReady { get; }
    }
}