using Beast.Claw;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainBiomeRender : ITerrainBiomeRender, IGameCycleListener
    {
        private struct RenderResult
        {
            public float2 renderPosition;
            public float viewDistance;
            public float textureYScale;
            public float cellPixelSize;
            public float snapGridSize;
            public float renderDistance;
            public float texelSize;
        }
        
        public bool IsReady => _isReady;
        
        public Texture Texture => _biomeTexture;
        public float2 TexelSize => _renderResult.texelSize;
        public float ViewDistance => _renderResult.viewDistance;
        public float RenderDistance => _renderResult.renderDistance;

        private readonly IScreen _screen;
        private readonly ITerrainBakedTexture _bakedTerrain;
        private readonly GraphicLibrary _library;
        private readonly ICameraTransform _camera;
        private readonly RenderConfig _config;

        private Material _renderMaterial;
        private RenderTexture _biomeTexture;
        private int2 _renderTextureForScreenSize;
        private RenderResult _renderResult;
        
        private bool _isReady;

        public TerrainBiomeRender(
            IScreen screen,
            ITerrainBakedTexture bakedTerrain,
            ICameraTransform camera,
            GraphicLibrary library,
            RenderConfig config)
        {
            _screen = screen;
            _bakedTerrain = bakedTerrain;
            _library = library;
            _camera = camera;
            _config = config;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
            handle.ListenUpdate(this, UpdateID.Render, _render);
        }

        public void Render(TerrainBiomeRenderTask task)
        {
            task.ValidateThrowException();
            
            _renderBiome(task);
        }

        private UniTask<OperationResult> _warmup()
        {
            _dispose();
            
            _renderMaterial = BGraphic.CreateMaterial(this, _library.terrain.biomeRender);

            _renderTextureForScreenSize = _screen.Size;

            float cos60 = BMath.Cos(_camera.RotationRad.roll);
            float sin45 = BMath.Sin(_camera.RotationRad.pitch);
            int textureSize = BMath.CeilToInt(BMath.Max(_screen.Width, _screen.Height) / cos60 * sin45);
            
            // We need extra +2 space, because of grid snapping.
            textureSize = BMath.CeilToInt(textureSize / (float)WorldTransform.CellTileCount + 2) *
                          WorldTransform.CellTileCount;

            RenderTextureData textureData = _config.terrain.biomeTexture.WithSize(textureSize);
            _biomeTexture = BGraphic.CreatePermRenderTexture(this, textureData);
            
            return OperationResult.SuccessSync();
        }
        
        public float2 WorldToUVOffset(float2 worldPoint)
        {
            float2 offset = (worldPoint - _renderResult.renderPosition) / _renderResult.renderDistance * 2;
            
            return (offset + 1) * 0.5f;
        }

        private void _render()
        {
            _isReady = false;
            
            if (!_bakedTerrain.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                _dispose();
                _warmup();
            }
            
            _renderResult = _renderBiome(new TerrainBiomeRenderTask
            {
                outputBiomeTexture = _biomeTexture,
                bakedTerrain = _bakedTerrain.Texture,
                renderPosition = _camera.Position,
                screenSize = _screen.Size,
                viewDistance = _camera.ViewDistance,
                useCameraScalingConstraint = true,
                useRenderPositionGridSnap = true,
            });
            
            _debugTrack(_renderResult);

            _isReady = true;
        }

        private RenderResult _renderBiome(TerrainBiomeRenderTask task)
        {
            task.ValidateThrowException();

            RenderResult stack;
            
            stack.textureYScale = task.outputBiomeTexture.height / (float)task.screenSize.y;
            stack.viewDistance = task.viewDistance;
            stack.cellPixelSize = task.outputBiomeTexture.height / stack.viewDistance / stack.textureYScale;
            
            if (task.useCameraScalingConstraint && stack.cellPixelSize > WorldTransform.CellTileCount)
            {
                stack.viewDistance *= stack.cellPixelSize / WorldTransform.CellTileCount;
                stack.cellPixelSize = WorldTransform.CellTileCount;
            }

            if (task.useRenderPositionGridSnap)
            {
                stack.snapGridSize = WorldTransform.CellTileCount / stack.cellPixelSize;
                stack.renderPosition = BMath.Floor(task.renderPosition / stack.snapGridSize) * stack.snapGridSize;
            }
            else
            {
                stack.snapGridSize = 0;
                stack.renderPosition = task.renderPosition;
            }
            
            stack.renderDistance = stack.viewDistance * stack.textureYScale;
            stack.texelSize = 1f / stack.renderDistance / WorldTransform.CellTileCount;
            
            _renderMaterial.SetFloat2("_OutputSize", stack.renderDistance);
            _renderMaterial.SetFloat2("_OutputCenter", stack.renderPosition);
            
            _renderMaterial.SetTexture("_TerrainTexture", task.bakedTerrain);
            _renderMaterial.SetFloat2("_TerrainTextureSize", task.bakedTerrain.Size());

            _renderMaterial.SetTexture("_NoiseMap", _library.terrain.biomeRenderNoise);
            _renderMaterial.SetFloat2("_NoiseMapSize", _library.terrain.biomeRenderNoise.Size());
            
            BGraphic.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = task.outputBiomeTexture
            });

            return stack;
        }
        
        private UniTask<OperationResult> _dispose()
        {
            _isReady = false;

            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _biomeTexture);

            return OperationResult.SuccessSync();
        }
        
        private void _debugTrack(RenderResult stack)
        {
            BDebug.Track(this, "biomeTextureSize", _biomeTexture.Size());
            BDebug.Track(this, nameof(stack.textureYScale), stack.textureYScale);
            BDebug.Track(this, nameof(stack.texelSize), stack.texelSize);
            BDebug.Track(this, nameof(stack.cellPixelSize), stack.cellPixelSize);
            BDebug.Track(this, nameof(stack.viewDistance), stack.viewDistance);
            BDebug.Track(this, nameof(stack.renderDistance), stack.renderDistance);
            BDebug.Track(this, nameof(stack.snapGridSize), stack.snapGridSize);
            BDebug.Track(this, nameof(stack.renderPosition), stack.renderPosition);
        }
    }
}