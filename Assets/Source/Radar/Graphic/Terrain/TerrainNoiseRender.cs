using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic.Terrain
{
    public class TerrainNoiseRender : ITerrainNoiseRender, IGameCycleListener
    {
        public bool IsReady => _isReady;
        public Texture RenderTexture => _renderTexture;
        public float RenderDistance => _renderDistance;
        
        private readonly GameModel _game;
        private readonly IScreen _screen;
        private readonly ITerrainBakedTexture _terrainTexture;
        private readonly ITerrainBaker _baker;
        private readonly GraphicLibrary _library;
        private readonly ICameraTransform _camera;

        private Material _renderMaterial;
        private RenderTexture _renderTexture;
        private float2 _renderCenter;
        private float _renderDistance;
        private float _renderTextureAspect;
        private int2 _renderTextureSize;
        private int2 _renderTextureForScreenSize;
        
        private bool _isReady;

        public TerrainNoiseRender(
            GameModel game,
            IScreen screen,
            ITerrainBakedTexture terrainTexture, 
            ITerrainBaker baker,
            GraphicLibrary library,
            ICameraTransform camera)
        {
            _game = game;
            _screen = screen;
            _terrainTexture = terrainTexture;
            _baker = baker;
            _library = library;
            _camera = camera;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
            handle.ListenUpdate(this, UpdateID.FrameEnd, FrameEnd);
        }

        private UniTask<OperationResult> Warmup()
        {
            Dispose();
            
            _renderMaterial = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                name = "Render",
                shader = _library.terrainNoiseShader,
            });

            _renderTextureForScreenSize = _screen.Size;
            _renderTextureSize.x = BMath.CeilToInt(_screen.Width / BMath.Cos(_camera.MaxRotationRad.roll));
            _renderTextureSize.y = BMath.CeilToInt(_screen.Height / BMath.Cos(_camera.MaxRotationRad.roll));
            _renderTextureAspect = _renderTextureSize.x / (float)_renderTextureSize.y;
            _renderTexture = _baker.Create(this, _renderTextureSize, false);
            
            return OperationResult.SuccessSync();
        }
        
        private UniTask<OperationResult> Dispose()
        {
            _isReady = false;
            
            if (_renderMaterial != null)
            {
                BeastCG.DisposeMaterial(_renderMaterial);
                _renderMaterial = null;
            }

            if (_renderTexture != null)
            {
                BeastCG.DisposeRenderTexture(_renderTexture);
                _renderTexture = null;
            }

            return OperationResult.SuccessSync();
        }

        private void FrameEnd()
        {
            _isReady = false;
            
            if (!_terrainTexture.IsReady || !_camera.IsReady)
            {
                return;
            }

            if (!_screen.Size.Equals(_renderTextureForScreenSize))
            {
                Dispose();
                Warmup();
            }
            
            float2x2 scale = IsometricCameraRotation.GetScaleMatrix(_camera.CurrentRotationRad.pitch);
            float2x2 rotation = IsometricCameraRotation.GetRotationMatrix(_camera.CurrentRotationRad.roll);

            _renderCenter = rotation.Multiply(scale.Multiply(_camera.Position));
            _renderDistance = _camera.TargetViewDistance / BMath.Cos(_camera.MaxRotationRad.roll);
            
            _renderMaterial.SetFloat("_OutputAspect", _renderTextureAspect);
            _renderMaterial.SetFloat("_OutputHeight", _renderDistance);
            _renderMaterial.SetVector("_OutputCenter", _renderCenter);
            
            _renderMaterial.SetTexture("_TerrainTexture", _terrainTexture.Texture);
            _renderMaterial.SetVector("_TerrainTextureSize", _game.terrain.size);
            
            _renderMaterial.SetMatrix("_UVMatrix0", scale.Inverse());
            _renderMaterial.SetMatrix("_UVMatrix1", rotation.Inverse());
            
#if BEAST_DEVELOPMENT
            _renderMaterial.SetTexture("_DebugGridTexture", _library.debug.terrainGrid);
            _renderMaterial.SetVector("_DebugGridTextureSize", _library.debug.terrainGrid.WidthHeightFloat());
#endif
            
            Texture2D worldDetailNoise = _library.worldNoiseTexture;
            _renderMaterial.SetTexture("_NoiseMap", worldDetailNoise);
            _renderMaterial.SetVector("_NoiseMapSize", worldDetailNoise.WidthHeightFloat());
            
            BeastCG.Blit(new BlitParameters
            {
                material = _renderMaterial,
                targetTexture = _renderTexture
            });

            _isReady = true;
        }
    }
}