using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    public interface ITerrainPreviewRender : IRender<TerrainPreviewRenderTask>
    {
        TerrainPreviewTexture CreateTexture(IController owner, int2 pixelSize);
    }
}