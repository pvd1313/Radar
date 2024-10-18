using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public class TerrainPreviewRenderExample : IGameCycleListener
    {
        private readonly ITerrainPreviewRender _previewRender;
        private readonly GraphicLibrary _library;

        private RenderTexture _screenTexture;
        private Material _unlitMaterial;
        
        // Read 3 steps below. Delete this class after preview integrated.

        public TerrainPreviewRenderExample(
            ITerrainPreviewRender previewRender, 
            GraphicLibrary library)
        {
            _previewRender = previewRender;
            _library = library;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, _warmup);
            handle.ListenOperation(this, OperationID.Dispose, _dispose);
            handle.ListenUpdate(this, UpdateID.Render, _render);
        }

        private UniTask<OperationResult> _warmup()
        {
            // 1. Allocate texture --------------------------------------------------//
            _screenTexture = _previewRender.CreateTexture(this, 256);
            // ---------------------------------------------------------------------//
            
#if DEVELOPMENT
            _unlitMaterial = BGraphic.CreateMaterial(this, _library.debug.unlitTextureShader);
#endif

            return OperationResult.SuccessSync();
        }
        
        private void _render()
        {
            if (Build.IS_PRODUCTION)
            {
                return;
            }
            
#if DEVELOPMENT
            if (!_library.debug.toggle0)
            {
                return;
            }
#endif
            
            // 2. Call each time terrain is modified.------------------------------//
            _previewRender.Render(new TerrainPreviewRenderTask
            {
                terrain = new TerrainData
                {
                    biomeScale01 = 0.5f,
                    seed01 = 0.31f,
                    size = 256,
                    water01 = 0.2f,
                },
                outputTexture = _screenTexture
            });
            // -------------------------------------------------------------------//

            _unlitMaterial.mainTexture = _screenTexture;
            
            BGraphic.DrawMeshDelayed(new DrawMeshTask
            {
                material = _unlitMaterial,
                subMeshIndex = 0,
                mesh = _library.quad,
                matrix = new Float4x4TRS
                {
                    rotation = quaternion.identity,
                    scale = 1,
                    translation = new float3(0, 0, RenderOrderZ.Max)
                }
            });
        }
        
        private UniTask<OperationResult> _dispose()
        {
            // 3. Dispose RenderTexture as soon as possible. (after window closed)
            //    They consume a lot of memory.
            BGraphic.TryDispose(ref _screenTexture);
            // -------------------------------------------------------------------// 
            BGraphic.TryDispose(ref _unlitMaterial);

            return OperationResult.SuccessSync();
        }
    }
}