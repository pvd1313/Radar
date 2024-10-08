using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public interface ITerrainNoiseRender : IReady
    {
        Texture RenderTexture { get; }
        float RenderDistance { get; }
    }
}