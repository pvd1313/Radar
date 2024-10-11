using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainDetailRender : IReady
    {
        Texture DetailTexture { get; }
    }
}