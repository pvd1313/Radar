using Beast.Claw;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
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
        private readonly GraphicConfig _config;

        public TerrainPreviewRender(
            IRender<TerrainBakeTask> baker, 
            IRender<TerrainBiomeRenderTask> biomeRender,
            IRender<TerrainColorRenderTask> colorRender,
            GraphicConfig config)
        {
            _baker = baker;
            _biomeRender = biomeRender;
            _colorRender = colorRender;
            _config = config;
        }
        
        public TerrainPreviewTexture CreateTexture<TOwner>(TOwner owner, int2 pixelSize) 
            where TOwner: class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            pixelSize.ValidateComponentsBiggerThanZeroThrowException(nameof(pixelSize));

            RenderTextureData bakerData = _config.terrain.bakerRenderTexture.WithSize(pixelSize);
            RenderTextureData biomeData = _config.terrain.biomeTexture.WithSize(pixelSize);
            RenderTextureData colorData = _config.terrain.colorTexture.WithSize(pixelSize);
            colorData.texture.filterMode = FilterMode.Bilinear;

            return new TerrainPreviewTexture
            {
                baker = BGraphic.CreatePermRenderTexture(owner, bakerData),
                biome = BGraphic.CreatePermRenderTexture(owner, biomeData),
                color = BGraphic.CreatePermRenderTexture(owner, colorData)
            };
        }

        public void Render(TerrainPreviewRenderTask task)
        {
            task.ValidateThrowException();

            _baker.Render(new TerrainBakeTask
            {
                terrain = task.terrain,
                quality01 = 0,
                outputTexture = task.texture.baker
            });

            _biomeRender.Render(new TerrainBiomeRenderTask
            {
                screenSize = task.texture.color.Size(),
                renderPosition = task.texture.color.Size().Multiply(0.5f),
                viewDistance = task.texture.color.height,
                bakedTerrain = task.texture.baker,
                outputBiomeTexture = task.texture.biome,
                isPreview = true
            });

            _colorRender.Render(new TerrainColorRenderTask
            {
                cameraViewDistance = task.texture.color.height,
                screenSize = task.texture.color.Size(),
                biomeTextureUVOffset = 0.5f,
                biomeViewDistance = task.texture.color.height,
                biomeTexture = task.texture.biome,
                outputToTexture = task.texture.color
            });
        }
    }
}