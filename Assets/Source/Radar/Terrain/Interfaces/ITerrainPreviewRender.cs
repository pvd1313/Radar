using Beast.Claw.Graphic;
using Unity.Mathematics;

namespace Beast.Radar.Terrain
{
    public interface ITerrainPreviewRender : IRender<TerrainPreviewRenderTask>
    {
        TerrainPreviewTexture CreateTexture<TOwner>(TOwner owner, int2 pixelSize) where TOwner: class;
    }
}