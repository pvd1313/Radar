using Beast.Claw.Attributes;
using Beast.Claw.Collections;
using Beast.Claw.Common;
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
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
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

            RoadSplineVariantJob variantJob = new()
            {
                inSplineMap = splineMap,
                outSplineMasks = BNative.TempJobHashSet<long>(splineMap.roadPoints.Length)
            };
            
            Profiler.BeginSample("RoadSplineVariantJob");
            variantJob.Run();
            Profiler.EndSample();
            
            BDebug.Track(this, "SplineVariantCount", variantJob.outSplineMasks.Count);
            
            variantJob.outSplineMasks.Dispose();
            
            return UniTask.CompletedTask;
        }

        public void Render(TerrainBiomePostProcess task)
        {
            Profiler.BeginSample("RoadTileRender.Render(TerrainBiomePostProcess task)");
            
            RoadSplineMap splineMap = _roadSplineRender.RoadMap;
            Bounds2D viewBounds = Bounds2D.FromPointWithRadius(task.renderPosition, task.renderDistance * 0.5f);
            NativeArray<int> roadsBuffer = BNative.TempJobArray<int>(16);
            NativeSlice<int> roads = _roadGrid.GetItems(viewBounds, roadsBuffer);
            NativeArray<RoadSplineSlice> roadSlices = BNative.TempJobArray<RoadSplineSlice>(roads.Length * 16);

            int totalCurveCount = 0;
            int totalSliceCount = 0;
            
            Profiler.BeginSample("SliceRoads");
            for (int i = 0; i < roads.Length; i++)
            {
                RoadSlicedSplineUtil.CutAABB(
                    splineMap, 
                    roads[i], 
                    viewBounds,
                    roadSlices.Slice(totalSliceCount, 16), 
                    out int sliceCount);
                
                for (int j = 0; j < sliceCount; j++)
                {
                    RoadSplineSlice slice = roadSlices[totalSliceCount + j];

                    totalCurveCount += slice.pointCount - 1;
                }
                
                totalSliceCount += sliceCount;
            }
            Profiler.EndSample();

            Profiler.BeginSample("PrepareCurves");
            NativeArray<int> curves = BNative.TempJobArray<int>(totalCurveCount);
            NativeArray<int> curveOffsets = BNative.TempJobArray<int>(totalCurveCount);
            NativeArray<int> curveBufferLimits = BNative.TempJobArray<int>(totalCurveCount);
            int curveIndex = 0;
            int estimatedPixelCount = 0;
            
            for (int i = 0; i < totalSliceCount; i++)
            {
                RoadSplineSlice roadSlice = roadSlices[i];
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
            Profiler.EndSample();

            RoadSplineRasterizationJob rasterizationJob = new()
            {
                inCurves = curves,
                inTextureSize = task.biomeTexture.Size(),
                inOffsets = curveOffsets,
                inBufferLimits = curveBufferLimits,
                inCurveOffset = task.renderPosition - new float2(task.renderDistance * 0.5f),
                inRoadMap = splineMap,
                inHalfPerPixel = HALF_PER_PIXEL,
                outPixelHalfs = BNative.TempJobArray<ushort>(estimatedPixelCount * HALF_PER_PIXEL),
                outPixelCount = BNative.TempJobArray<int>(totalCurveCount)
            };
            
            Profiler.BeginSample("RoadSplineRasterizationJob");
            rasterizationJob.Schedule(totalCurveCount, 4).Complete();
            Profiler.EndSample();

            Profiler.BeginSample("MemCopyPixelBuffer");
            int totalPixelCount = 0;
            for (int i = 0; i < totalCurveCount; i++)
            {
                totalPixelCount += rasterizationJob.outPixelCount[i];
            }

            int2 textureSize = BGraphic.GetDataTextureSize(totalPixelCount);
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
            Profiler.EndSample();

            Profiler.BeginSample("ApplyTexture");
            HalfStream stream = HalfStream.CreateReadOnly(pixelsRaw, totalPixelCount, HALF_PER_PIXEL, 1);
            Texture2D texture = _textureProvider.GetTexture(textureSize);
            stream.CopyToTexture(texture);
            Profiler.EndSample();
            
            Profiler.BeginSample("SetupMaterial");
            _renderMaterial.SetTexture("_DataTexture", texture);
            _renderMaterial.SetFloat2("_DataTextureSize", textureSize);
            _renderMaterial.SetFloat("_PixelColor", (int)ETerrainTile.Road);
            Profiler.EndSample();
            
            Profiler.BeginSample("SetupCommandBuffer");
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
            Profiler.EndSample();
            
            Profiler.BeginSample("ExecuteCommandBuffer");
            BGraphic.ExecuteBuffer(_commandBuffer);
            Profiler.EndSample();

            curves.Dispose();
            curveOffsets.Dispose();
            roadsBuffer.Dispose();
            curveBufferLimits.Dispose();
            pixelsRaw.Dispose();
            roadSlices.Dispose();

            rasterizationJob.outPixelHalfs.Dispose();
            rasterizationJob.outPixelCount.Dispose();
            
            Profiler.EndSample();
            
            BDebug.Track(this, "CurveCount", totalCurveCount);
            BDebug.Track(this, "VisibleRoads", roads.Length);
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