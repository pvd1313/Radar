using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainDetailRender : IReady
    {
        Texture RenderTexture { get; }
        float RenderDistance { get; }
    }
}