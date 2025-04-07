using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Claw.Validation;
using Beast.Radar.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBiomeRender : Actor, ITerrainBiomeRender
    {
        private struct PostProcessor
        {
            public ITerrainBiomePostProcessor processor;
            public int priority;
        }
        
        public bool IsReady => _isReady;
        
        public Texture Texture => _biomeTexture;
        public float2 TexelSize => _renderResult.texelSize;
        public float ViewDistance => _renderResult.viewDistance;

        [Required] private readonly IScreen _screen;
        [Required] private readonly ITerrainBakedFbm _bakedTerrain;
        [Required] private readonly GraphicLibrary _library;
        [Required] private readonly ICameraTransform _camera;
        [Required] private readonly TerrainRenderConfig _config;

        private Material _renderMaterial;
        private RenderTexture _biomeTexture;
        private int2 _renderTextureForScreenSize;
        private TerrainBiomeRenderResult _renderResult;
        private List<PostProcessor> _postProcessors;
        
        private bool _isReady;

        public void Render(TerrainBiomeRenderTask task)
        {
            task.ValidateThrowException(nameof(task));
            
            _renderBiome(task);
        }
        
        public float2 WorldToUVOffset(float2 worldPoint)
        {
            float2 offset = (worldPoint - _renderResult.renderPosition) / _renderResult.renderDistance * 2;
            
            return (offset + 1) * 0.5f;
        }

        public void RegisterPostProcessor(ITerrainBiomePostProcessor processor, ETerrainBiomePostProcessorPriority priority)
        {
            int i = 0;
            for (; i < _postProcessors.Count; i++)
            {
                if (_postProcessors[i].priority < (int)priority)
                {
                    break;
                }
            }
            
            _postProcessors.Insert(i, new PostProcessor
            {
                priority = (int) priority,
                processor = processor
            });
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrain.biomeRender);

            _renderTextureForScreenSize = _screen.Size;

            _postProcessors ??= new List<PostProcessor>();

            int textureSize = (int)BMath.Max(_screen.Width(), _screen.Height());

            if (!_camera.RotationRad.IsZero())
            {
                float cos60 = BMath.Cos(_camera.RotationRad.roll);
                float sin45 = BMath.Sin(_camera.RotationRad.pitch);
                textureSize = BMath.CeilToInt(textureSize / cos60 * sin45);
            }
            
            // Will not work when aspect < 1.5
            textureSize = BMath.CeilToInt(textureSize / (float)BWorld.TILES_PER_CELL) * BWorld.TILES_PER_CELL;

            RenderTextureData textureData = _config.biomeRenderTexture.WithSize(textureSize);
            _biomeTexture = BGraphic.CreatePermRenderTexture(this, textureData);

            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            _renderResult.renderPosition = new float2(float.MaxValue, float.MaxValue);

            return UniTask.CompletedTask;
        }

        protected override void _render(BlockingContext context)
        {
            if (!_bakedTerrain.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                _dispose(default);
                _warmup(default);
            }

            _isReady = false;
            
            _renderResult = _renderBiome(new TerrainBiomeRenderTask
            {
                outputBiomeTexture = _biomeTexture,
                bakedTerrain = _bakedTerrain.Texture,
                renderPosition = _camera.Position,
                screenSize = _screen.Size,
                viewDistance = _camera.ViewDistance,
                useDynamicConstraints = true,
                usePostProcessors = true,
            });
            
            _debugTrack(_renderResult);

            _isReady = true;
        }

        private TerrainBiomeRenderResult _renderBiome(TerrainBiomeRenderTask task)
        {
            task.ValidateThrowException(nameof(task));

            TerrainBiomeRenderResult data;
            
            data.textureYScale = task.outputBiomeTexture.height / (float)task.screenSize.y;
            data.viewDistance = task.viewDistance;
            data.cellPixelSize = task.outputBiomeTexture.height / data.viewDistance / data.textureYScale;

            bool isCloseRendering = false;
            
            // Upscale when very close to terrain.
            if (task.useDynamicConstraints && data.cellPixelSize > BWorld.TILES_PER_CELL)
            {
                data.viewDistance *= data.cellPixelSize / BWorld.TILES_PER_CELL;
                data.cellPixelSize = BWorld.TILES_PER_CELL;
                isCloseRendering = true;
            }

            // Snap to grid.
            if (task.useDynamicConstraints)
            {
                data.snapGridSize = BWorld.TILES_PER_CELL / data.cellPixelSize;
                data.renderPosition = BMath.Floor(task.renderPosition / data.snapGridSize) * data.snapGridSize;
            }
            else
            {
                data.snapGridSize = 0;
                data.renderPosition = task.renderPosition;
            }
            
            data.renderDistance = data.viewDistance * data.textureYScale;
            data.texelSize = 1f / data.renderDistance / BWorld.TILES_PER_CELL;
            
            // LODs.
            if (!task.useDynamicConstraints || data.cellPixelSize < _config.noiseSamplerLOD1)
            {
                data.noiseSamplerLOD = 1;
            }
            else
            {
                FloatMinMax minMax = new(_config.noiseSamplerLOD1, _config.noiseSamplerLOD0);
                data.noiseSamplerLOD = BMath.Max(0, 1 - BMath.InverseLerp(minMax, data.cellPixelSize));
            }
            
            // Prevent rendering when texture not moving.
            if (task.useDynamicConstraints)
            {
                if (BMath.Approximately(data.renderPosition, _renderResult.renderPosition))
                {
                    BDebug.Track(this, "textureIsStatic", true);
                    return data;
                }
                
                BDebug.Track(this, "textureIsStatic", false);
            }
            
            _renderMaterial.SetFloat2("_OutputSize", data.renderDistance);
            _renderMaterial.SetFloat2("_OutputCenter", data.renderPosition);
            
            _renderMaterial.SetTexture("_TerrainTexture", task.bakedTerrain);
            _renderMaterial.SetFloat2("_TerrainTextureSize", task.bakedTerrain.Size());
            
            _renderMaterial.SetTexture("_IndexMap", _library.terrain.atlas.IndexTexture);

            _renderMaterial.SetTexture("_NoiseMap", _library.terrain.biomeRenderNoise);
            _renderMaterial.SetFloat2("_NoiseMapSize", _library.terrain.biomeRenderNoise.Size());

            _setNoisePassLOD(_renderMaterial, data.noiseSamplerLOD);

            BGraphic.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = task.outputBiomeTexture
            });

            if (task.usePostProcessors && isCloseRendering)
            {
                foreach (PostProcessor postProcessor in _postProcessors)
                {
                    BProfiler.BeginSample(postProcessor.processor.Meta.name);
                    
                    postProcessor.processor.Render(new TerrainBiomePostProcess
                    {
                        renderDistance = data.renderDistance,
                        viewDistance = data.viewDistance,
                        renderPosition = data.renderPosition,
                        biomeTexture = task.outputBiomeTexture
                    });
                    
                    BProfiler.EndSample();
                }
            }

            return data;
        }

        private static void _setNoisePassLOD(Material material, float lod)
        {
            int lodInt = BMath.FloorToInt(lod);
            
            material.SetKeyword("NOISE_SAMPLER_LOD_0", lodInt == 0);
            material.SetKeyword("NOISE_SAMPLER_LOD_1", lodInt == 1);
            material.SetFloat("_NoiseSamplerLOD", lod);
        }

        protected override void _dispose(BlockingContext context)
        {
            _isReady = false;

            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _biomeTexture);
        }
        
        private void _debugTrack(TerrainBiomeRenderResult stack)
        {
            BDebug.Track(this, "biomeTextureSize", _biomeTexture.Size());
            BDebug.Track(this, "terrainTextureSize", _bakedTerrain.Texture.Size());
            BDebug.Track(this, "terrainTextureTexelSize", 1f / (float2)_biomeTexture.Size());
            BDebug.Track(this, nameof(stack.noiseSamplerLOD), stack.noiseSamplerLOD);
            BDebug.Track(this, nameof(stack.textureYScale), stack.textureYScale);
            BDebug.Track(this, nameof(stack.texelSize), stack.texelSize);
            BDebug.Track(this, nameof(stack.cellPixelSize), stack.cellPixelSize);
            BDebug.Track(this, nameof(stack.viewDistance), stack.viewDistance);
            BDebug.Track(this, nameof(stack.renderDistance), stack.renderDistance);
            BDebug.Track(this, nameof(stack.snapGridSize), stack.snapGridSize);
            BDebug.Track(this, nameof(stack.renderPosition), stack.renderPosition);
            BDebug.Track(this, nameof(stack.renderPosition), stack.renderPosition);
        }
    }
}