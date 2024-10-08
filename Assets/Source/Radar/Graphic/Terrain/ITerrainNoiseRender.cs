using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public interface ITerrainNoiseRender : IReady
    {
        Texture RenderedTexture { get; }
    }
}