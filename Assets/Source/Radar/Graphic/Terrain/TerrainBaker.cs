using Beast.Claw.CG;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Graphic
{
    public class TerrainBaker : ITerrainBaker, IGameCycleListener
    {
        private const RenderTextureFormat TextureFormat = RenderTextureFormat.RFloat;
        private const TextureWrapMode TextureWrapMode = UnityEngine.TextureWrapMode.Clamp;
        private const FilterMode TextureFilterMode = FilterMode.Point;
        
        private readonly GraphicLibrary _graphicLibrary;

        private Material _material;

        public TerrainBaker(GraphicLibrary graphicLibrary)
        {
            _graphicLibrary = graphicLibrary;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Warmup, Warmup);
            handle.ListenOperation(this, OperationID.Dispose, Dispose);
        }
        
        private UniTask<OperationResult> Warmup()
        {
            _material = BeastCG.CreateMaterial(this, new MaterialParameters
            {
                shader = _graphicLibrary.worldMapBakeShader,
                name = "TerrainBake",
            });
            
            return OperationResult.SuccessSync();
        }
        
        private UniTask<OperationResult> Dispose()
        {
            if (_material != null)
            {
                BeastCG.ReleaseMaterial(_material);
                _material = null;
            }

            return OperationResult.SuccessSync();
        }

        public RenderTexture CreateTexture<TOwner>(TOwner owner, int2 size, bool isTemporary)
        {
            if (isTemporary)
            {
                return BeastCG.CreateTemporaryRenderTexture(owner, new NewTempRenderTextureData
                {
                    antiAliasing = 1,
                    filterMode = TextureFilterMode,
                    format = TextureFormat,
                    size = size,
                    wrapMode = TextureWrapMode
                });
            }

            return BeastCG.CreatePermanentRenderTexture(owner, new NewPermaRenderTextureData
            {
                mipCount = 0,
                depth = 0,
                size = size,
                wrapMode = TextureWrapMode,
                filterMode = TextureFilterMode,
                format = TextureFormat
            });
        }

        public void BakeTexture(TerrainBakerInput input)
        {
            input.ValidateThrowException();
            
            _material.SetFloat("_Flood01", input.waterFlood01);
            _material.SetFloat("_Seed01", input.seed01);
            _material.SetFloat("_Scale99", input.biomeScale99);
            
            _material.SetVector(
                "_TextureSize", 
                new Vector2(input.targetTexture.width, input.targetTexture.height));
            
            BeastCG.Blit(new BlitParameters
            {
                targetTexture = input.targetTexture,
                material = _material,
                shaderPass = 0
            });
        }
    }
}