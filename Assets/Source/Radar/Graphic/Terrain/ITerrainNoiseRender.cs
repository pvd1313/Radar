using UnityEngine;

namespace Beast.Radar.Graphic
{
    public interface ITerrainNoiseRender
    {
        bool IsReady { get; }
        Texture RenderedTexture { get; }
    }
}