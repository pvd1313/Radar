using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Claw.Text;
using Beast.Claw.Validation;
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
        
        public TerrainPreviewTexture CreateTexture(IController owner, int2 pixelSize) 
        {
            owner.ValidateNotNullThrowException(nameof(owner));

            if (Build.IS_DEVELOPMENT)
            {
                if (pixelSize.x <= 0 || pixelSize.y <= 0)
                {
                    string message = $"{nameof(pixelSize).Q()} components must be bigger than zero, but was: {pixelSize}.";
                    throw new ValidationException(message);
                }
            }
            
            RenderTextureData bakerData = _config.terrain.fbmRenderTexture.WithSize(pixelSize);
            RenderTextureData biomeData = _config.terrain.biomeRenderTexture.WithSize(pixelSize);
            RenderTextureData colorData = _config.terrain.colorRenderTexture.WithSize(pixelSize);
            colorData.filterMode = FilterMode.Bilinear;

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
                renderPosition = task.texture.color.Size().Mul(0.5f),
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