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
        private readonly IRender<TerrainFbmRenderTask> _fbmRender;
        private readonly IRender<TerrainBiomeRenderTask> _biomeRender;
        private readonly IRender<TerrainColorRenderTask> _colorRender;
        private readonly GraphicConfig _config;

        public TerrainPreviewRender(
            IRender<TerrainFbmRenderTask> fbmRender, 
            IRender<TerrainBiomeRenderTask> biomeRender,
            IRender<TerrainColorRenderTask> colorRender,
            GraphicConfig config)
        {
            _fbmRender = fbmRender;
            _biomeRender = biomeRender;
            _colorRender = colorRender;
            _config = config;
        }
        
        public TerrainPreviewTexture CreateTexture<TOwner>(TOwner owner, int2 pixelSize) 
            where TOwner: class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            pixelSize.ValidateComponentsBiggerThanZeroThrowException(nameof(pixelSize));

            RenderTextureData bakerData = _config.terrain.fbmRenderTexture.WithSize(pixelSize);
            RenderTextureData biomeData = _config.terrain.biomeRenderTexture.WithSize(pixelSize);
            RenderTextureData colorData = _config.terrain.colorTexture.WithSize(pixelSize);
            colorData.texture.filterMode = FilterMode.Bilinear;

            return new TerrainPreviewTexture
            {
                fbm = BGraphic.CreatePermRenderTexture(owner, bakerData),
                biome = BGraphic.CreatePermRenderTexture(owner, biomeData),
                color = BGraphic.CreatePermRenderTexture(owner, colorData)
            };
        }

        public void Render(TerrainPreviewRenderTask task)
        {
            task.ValidateThrowException(nameof(task));

            _fbmRender.Render(new TerrainFbmRenderTask
            {
                terrain = task.terrain,
                quality01 = 0,
                outputTexture = task.texture.fbm
            });

            _biomeRender.Render(new TerrainBiomeRenderTask
            {
                screenSize = task.texture.color.Size(),
                renderPosition = task.texture.color.Size().Multiply(0.5f),
                viewDistance = task.texture.color.height,
                bakedTerrain = task.texture.fbm,
                outputBiomeTexture = task.texture.biome,
                useDynamicConstraints = false
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