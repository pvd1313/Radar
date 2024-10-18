using Beast.Claw.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public interface ITerrainPreviewRender : IRender<TerrainPreviewRenderTask>
    {
        RenderTexture CreateTexture<TOwner>(TOwner owner, int2 pixelSize);
    }
}