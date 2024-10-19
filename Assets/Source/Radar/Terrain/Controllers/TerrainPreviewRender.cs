using Beast.Claw;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Graphic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainPreviewRender : ITerrainPreviewRender
    {
        private readonly IRender<TerrainBakeTask> _baker;
        private readonly IRender<TerrainBiomeRenderTask> _biomeRender;
        private readonly IRender<TerrainColorRenderTask> _colorRender;
        private readonly RenderConfig _config;

        public TerrainPreviewRender(
            IRender<TerrainBakeTask> baker, 
            IRender<TerrainBiomeRenderTask> biomeRender,
            IRender<TerrainColorRenderTask> colorRender,
            RenderConfig config)
        {
            _baker = baker;
            _biomeRender = biomeRender;
            _colorRender = colorRender;
            _config = config;
        }
        
        public RenderTexture CreateTexture<TOwner>(TOwner owner, int2 pixelSize) 
            where TOwner: class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            pixelSize.ValidateComponentsBiggerZeroThrowException(nameof(pixelSize));

            RenderTextureData textureData = _config.terrain.colorTexture.WithSize(pixelSize);
            
            return BGraphic.CreatePermRenderTexture(owner, textureData);
        }
        
        public void Render(TerrainPreviewRenderTask task)
        {
            task.ValidateThrowException();
            
            RenderTexture bakedTexture = null;
            RenderTexture biomeTexture = null;
            
            try
            {
                RenderTextureData bakerData = _config.terrain.bakedTexture.WithSize(task.terrain.terrainSize);
                bakedTexture = BGraphic.CreateTempRenderTexture(this, bakerData);
                _baker.Render(new TerrainBakeTask
                {
                    terrain = task.terrain,
                    outputTexture = bakedTexture
                });

                RenderTextureData biomeData = _config.terrain.biomeTexture.WithSize(task.outputTexture);
                biomeTexture = BGraphic.CreateTempRenderTexture(this, biomeData);
                _biomeRender.Render(new TerrainBiomeRenderTask
                {
                    viewDistance = task.terrain.terrainSize.y,
                    bakedTerrain = bakedTexture,
                    outputBiomeTexture = biomeTexture,
                    renderPosition = task.terrain.terrainSize.Multiply(0.5f),
                    screenSize = task.outputTexture.Size()
                });
                
                _colorRender.Render(new TerrainColorRenderTask
                {
                    biomeTexture = biomeTexture,
                    biomeTextureUVOffset = 0.5f,
                    biomeViewDistance = task.terrain.terrainSize.y,
                    cameraViewDistance = task.terrain.terrainSize.y,
                    screenSize = task.outputTexture.Size(),
                    outputToTexture = task.outputTexture, 
                });
            }
            finally
            {
                BGraphic.TryDispose(ref bakedTexture);
                BGraphic.TryDispose(ref biomeTexture);
            }
        }
    }
}