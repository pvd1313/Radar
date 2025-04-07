using Beast.Claw.Attributes;
using Beast.Claw.Collections;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.Graphic;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class RoadTileRender : Actor, ITerrainBiomePostProcessor
    {
        private const int HALF_PER_PIXEL = 2;
        
        [Required] private ITerrainBiomeRender _biomeRender;
        [Required] private IRoadSplineRender _roadSplineRender;
        [Required] private IWorldManager _worldManager;
        [Required] private RoadTileRenderConfig _config;
        [Required] private GraphicLibrary _library;

        private TerrainData _backedForTerrain;
        private CellGrid<int> _roadGrid;
        private HalfTextureProvider _textureProvider;
        private Material _renderMaterial;
        private CommandBuffer _commandBuffer;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _biomeRender.RegisterPostProcessor(this, ETerrainBiomePostProcessorPriority.Road);

            _textureProvider = new HalfTextureProvider(this, "RoadTiles", HALF_PER_PIXEL);
            _renderMaterial = BGraphic.CreateMaterial(this, _config.pixelPlotShader);
            _commandBuffer = BGraphic.CreateCommandBuffer(this, nameof(RoadTileRender));
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (_backedForTerrain.IsSame(terrain))
            {
                return UniTask.CompletedTask;
            }

            _backedForTerrain = terrain;
            
            _roadGrid.Dispose();

            _roadGrid = new CellGrid<int>(_config.cellSize, 256, Allocator.Persistent);

            RoadSplineMap splineMap = _roadSplineRender.RoadMap;
            
            for (int i = 0; i < splineMap.roads.Length; i++)
            {
                _roadGrid.AddItem(i, splineMap.roads[i].bounds);
            }

            return UniTask.CompletedTask;
        }

        public void Render(TerrainBiomePostProcess task)
        {
            RoadSplineMap splineMap = _roadSplineRender.RoadMap;

            RoadBoundsSliceJob roadBoundsSliceJob = new()
            {
                inRoadMap = splineMap,
                inBounds = Bounds2D.FromPointWithRadius(task.renderPosition, task.viewDistance * 0.5f),
                inRoadGrid = _roadGrid,
                outRoadSliceCount = BNative.TempJobReference<int>(),
                outRoadCurveCount = BNative.TempJobReference<int>(),
                outRoadSlices = BNative.TempJobArray<RoadSplineSlice>(16)
            };
            
            roadBoundsSliceJob.Invoke();

            int totalCurveCount = roadBoundsSliceJob.outRoadCurveCount.Value;
            int totalSliceCount = roadBoundsSliceJob.outRoadSliceCount.Value;
            
            BProfiler.BeginSample("PrepareCurves");
            NativeArray<int> curves = BNative.TempJobArray<int>(totalCurveCount);
            NativeArray<int> curveOffsets = BNative.TempJobArray<int>(totalCurveCount);
            NativeArray<int> curveBufferLimits = BNative.TempJobArray<int>(totalCurveCount);
            int curveIndex = 0;
            int estimatedPixelCount = 0;
            
            for (int i = 0; i < totalSliceCount; i++)
            {
                RoadSplineSlice roadSlice = roadBoundsSliceJob.outRoadSlices[i];
                RoadSpline road = splineMap.roads[roadSlice.roadIndex];
                int firstPointIndex = roadSlice.firstPointIndex + road.firstPointIndex;

                for (int j = 0; j < roadSlice.pointCount - 1; j++)
                {
                    float curveLength = splineMap.curveLengths[firstPointIndex + j];
                    int curvePixelCount = BMath.CeilToInt(curveLength * BWorld.TILES_PER_CELL);
                    
                    curves[curveIndex] = firstPointIndex + j; 
                    curveOffsets[curveIndex] = estimatedPixelCount * HALF_PER_PIXEL;
                    curveBufferLimits[curveIndex] = curvePixelCount * HALF_PER_PIXEL;
                   
                    estimatedPixelCount += curvePixelCount;
                    
                    curveIndex++;
                }
            }
            BProfiler.EndSample();

            // Only visible pixels
            float viewRadius = task.viewDistance * 0.5f;
            float textureOffset0 = (task.renderDistance * 0.5f - viewRadius) / task.renderDistance;
            float textureOffset1 = (task.renderDistance * 0.5f + viewRadius) / task.renderDistance;
            float2 textureMin = BMath.FloorToInt(task.biomeTexture.Size().Mul(textureOffset0));
            float2 textureMax = BMath.FloorToInt(task.biomeTexture.Size().Mul(textureOffset1));

            RoadSplineRasterizationJob rasterizationJob = new()
            {
                inCurves = curves,
                inTextureBounds = Bounds2D.FromTwoPoints(textureMin, textureMax),
                inOffsets = curveOffsets,
                inBufferLimits = curveBufferLimits,
                inCurveOffset = task.renderPosition - new float2(task.renderDistance * 0.5f),
                inRoadMap = _roadSplineRender.RoadMap,
                inHalfPerPixel = HALF_PER_PIXEL,
                outPixelHalfs = BNative.TempJobArray<ushort>(estimatedPixelCount * HALF_PER_PIXEL),
                outPixelCount = BNative.TempJobArray<int>(totalCurveCount)
            };
            
            rasterizationJob.Invoke(totalCurveCount, 4);

            BProfiler.BeginSample("MemCopyPixelBuffer");
            int totalPixelCount = 0;
            for (int i = 0; i < totalCurveCount; i++)
            {
                totalPixelCount += rasterizationJob.outPixelCount[i];
            }

            int2 textureSize = 0;

            if (totalPixelCount > 0)
            {
                textureSize = BGraphic.GetDataTextureSize(totalPixelCount);
                NativeArray<ushort> pixelsRaw =
                    BNative.TempJobArray<ushort>(textureSize.x * textureSize.y * HALF_PER_PIXEL);
                
                int writeOffset = 0;
                
                for (int i = 0; i < totalCurveCount; i++)
                {
                    int halfCount = rasterizationJob.outPixelCount[i] * HALF_PER_PIXEL;
                
                    BNative.Copy(
                        rasterizationJob.outPixelHalfs,
                        pixelsRaw, 
                        curveOffsets[i], 
                        writeOffset, 
                        halfCount);

                    writeOffset += halfCount;
                }
                BProfiler.EndSample();

                BProfiler.BeginSample("ApplyTexture");
                HalfStream stream = HalfStream.CreateReadOnly(pixelsRaw, totalPixelCount, HALF_PER_PIXEL, 1);
                Texture2D texture = _textureProvider.GetTexture(textureSize);
                stream.CopyToTexture(texture);
                BProfiler.EndSample();
            
                BProfiler.BeginSample("SetupMaterial");
                _renderMaterial.SetTexture("_DataTexture", texture);
                _renderMaterial.SetFloat2("_DataTextureSize", textureSize);
                _renderMaterial.SetFloat("_PixelColor", (int)ETerrainTile.Road);
                BProfiler.EndSample();
            
                BProfiler.BeginSample("SetupCommandBuffer");
                _commandBuffer.Clear();
                _commandBuffer.SetRenderTarget(task.biomeTexture);

                float4x4 projection =
                    float4x4.OrthoOffCenter(
                        0, 
                        task.biomeTexture.width, 
                        0, 
                        task.biomeTexture.height, 
                        0, 
                        1);
            
                _commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, projection);
                _commandBuffer.DrawMeshInstancedProcedural(_library.quad, 0, _renderMaterial, 0, totalPixelCount);
                BProfiler.EndSample();
            
                BProfiler.BeginSample("ExecuteCommandBuffer");
                BGraphic.ExecuteBuffer(_commandBuffer);
                BProfiler.EndSample();
                
                pixelsRaw.Dispose();
            }

            roadBoundsSliceJob.outRoadCurveCount.Dispose();
            roadBoundsSliceJob.outRoadSliceCount.Dispose();
            roadBoundsSliceJob.outRoadSlices.Dispose();

            curves.Dispose();
            curveOffsets.Dispose();
            curveBufferLimits.Dispose();

            rasterizationJob.outPixelHalfs.Dispose();
            rasterizationJob.outPixelCount.Dispose();
            
            BDebug.Track(this, "CurveCount", totalCurveCount);
            BDebug.Track(this, "EstimatedPixelCount", estimatedPixelCount);
            BDebug.Track(this, "TotalPixelCount", totalPixelCount);
            BDebug.Track(this, "TextureSize", textureSize);
        }

        protected override void _dispose(BlockingContext obj)
        {
            _roadGrid.Dispose();
            _textureProvider.Dispose();
            BGraphic.TryDispose(ref _renderMaterial);
            BGraphic.TryDispose(ref _commandBuffer);
        }
    }
}