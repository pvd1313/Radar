using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainBakedTexture : IReady
    {
        public RenderTexture Texture { get; }
    }
}