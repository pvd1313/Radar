using System.Collections.Generic;
using System.Diagnostics;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Claw.Math.CCL;
using Beast.Radar.Navigation;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class RoadSpawner : Actor, IRoadSpawner
    {
        public NativeSlice<RoadInfo> Roads => _roads.Slice();
        
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly RoadSpawnConfig _config;
        [Required] private readonly IGizmoManager _gizmoManager;
        
        private TerrainData _bakedForTerrain;
        private IGizmoHandle _gizmo;
        private NativeArray<ushort2> _roadPoints;
        private NativeArray<RoadInfo> _roads;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            TerrainData terrain = _worldManager.GetCurrentWorld().terrain;

            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            _bakedForTerrain = terrain;

            if (_roads.IsCreated)
            {
                _roads.Dispose();
                _roadPoints.Dispose();
            }
            
            _config.buildCost.ToByteArray(out ByteArray16 cellBiomeCost, out ByteArray16 cellFeatureCost);
            
            Road8KMJob road8KMJob = new()
            {
                inCellBiomeCost = cellBiomeCost,
                inCellFeatureCost = cellFeatureCost,
                inCities = _citySpawner.Cities,
                inOutTerrain8KM = _biome.Terrain8kmCell,
                inSpawnPriority = _config.spawnPriority,
                outPoints = BNative.TempJobList<ushort2>(2048),
                outJunctions = new NativeHashMap<ushort2, ushort2>(256, Allocator.TempJob),
                outSegments = BNative.TempJobList<ushort>(256),
            };
            
            Profiler.BeginSample("RoadSegment8KMJob");
            road8KMJob.Run();
            Profiler.EndSample();
            
            int maxSegmentSize8KM = BMath.CeilToInt(_config.maxSegmentLength / (float)_biome.Terrain8kmCell.cellSize);

            NativeList<RoadSegment> segmentsToSearch = _processSegments8KM(
                road8KMJob, 
                _biome.Terrain1kmCell.TextureSize, 
                maxSegmentSize8KM);

            int maxPointPerSegment = BMath.CeilToInt(maxSegmentSize8KM * _biome.Terrain8kmCell.cellSize * 2f);

            Road1KMJob road1KMJob = new()
            {
                motionCost = new TerrainMotionCostComputer
                {
                    cellBiomeCost = cellBiomeCost,
                    cellFeatureCost = cellFeatureCost,
                    terrain1KMTexture = _biome.Terrain1kmCell,
                    use8KMTerrainTexture = false,
                    useNoiseTexture = true,
                    noiseTexture = new NativeTexture<byte>(_config.noiseTexture),
                    noiseStrength = _config.noiseStrength,
                    noiseRange = _config.noiseRange
                },
                inSegments = segmentsToSearch.AsArray(),
                inMaxPointPerSegment = maxPointPerSegment,
                outPoints = BNative.TempJobArray<ushort2>(maxPointPerSegment * segmentsToSearch.Length),
                outSegments = BNative.PermArray<RoadSegment>(segmentsToSearch.Length),
                outSegmentPointCount = BNative.TempJobArray<int>(segmentsToSearch.Length)
            };
            
            Profiler.BeginSample("RoadSegment1KMJob");
            road1KMJob.Schedule(segmentsToSearch.Length, 8).Complete();
            Profiler.EndSample();

            Profiler.BeginSample("CollectRoads");
            int totalPointCount = 0;
            foreach (int segmentPointCount in road1KMJob.outSegmentPointCount)
            {
                totalPointCount += segmentPointCount;
            }

            NativeArray<ushort2> points = BNative.PermArray<ushort2>(totalPointCount);
            NativeArray<RoadInfo> roads = BNative.PermArray<RoadInfo>(road1KMJob.outSegments.Length);
            
            int writeOffset = 0;
            int readOffset = 0;
            for (int i = 0; i < road1KMJob.outSegments.Length; i++)
            {
                RoadSegment segment = road1KMJob.outSegments[i];
                int segmentPointCount = road1KMJob.outSegmentPointCount[i];

                NativeSlice<ushort2> target = points.Slice(writeOffset, segmentPointCount);
                NativeSlice<ushort2> source = road1KMJob.outPoints.Slice(readOffset, segmentPointCount);
                
                target.CopyFrom(source);

                roads[i] = new RoadInfo
                {
                    points = target,
                    startPoint = segment.point0,
                    endPoint = segment.point1,
                    bounds = segment.bounds,
                };
                
                writeOffset += segmentPointCount;
                readOffset += maxPointPerSegment;
            }
            Profiler.EndSample();

            BDebug.Track(this, "RoadCount", roads.Length);
            BDebug.Track(this, "PointCount", points.Length);

            _gizmo.Clear();

            // _debugDrawPoints8KM(road8KMJob.outPoints);

            _debugDrawRoads(roads);

            road8KMJob.outPoints.Dispose();
            road8KMJob.outJunctions.Dispose();
            road8KMJob.outSegments.Dispose();
            segmentsToSearch.Dispose();

            road1KMJob.outPoints.Dispose();
            road1KMJob.outSegmentPointCount.Dispose();
            road1KMJob.outSegments.Dispose();

            _roads = roads;
            _roadPoints = points;
            
            return UniTask.CompletedTask;
        }
        
        private NativeList<RoadSegment> _processSegments8KM(Road8KMJob job, int terrainSize, int maxSegmentSize)
        {
            NativeList<RoadSegment> resultSegments = BNative.TempJobList<RoadSegment>(256);

            using NativeList<float2> km8Points = BNative.TempJobList<float2>(16);

            int pointOffset = 0;
            
            for (int segID = 0; segID < job.outSegments.Length; segID++)
            {
                int segPointCount = job.outSegments[segID];
                
                int pointIndex0 = pointOffset;
                ushort2 startPoint8KM = job.outPoints[pointIndex0];
                ushort2 junction0 = job.outJunctions[startPoint8KM];
                
                km8Points.Clear();
                km8Points.Add(job.inOutTerrain8KM.GetCellCenterWorld(startPoint8KM));
                
                int pointIndex1 = pointIndex0 + 1;
                
                pointOffset += segPointCount;

                while (pointIndex0 < pointOffset - 1)
                {
                    int pointCount = pointIndex1 - pointIndex0;

                    ushort2 endPoint8K = job.outPoints[pointIndex1];
                    km8Points.Add(job.inOutTerrain8KM.GetCellCenterWorld(endPoint8K));
                    
                    bool endIsJunction = job.outJunctions.TryGetValue(endPoint8K, out ushort2 junction1);

                    if (pointCount >= BMath.Min(maxSegmentSize, segPointCount) || endIsJunction)
                    {
                        if (!endIsJunction)
                        {
                            junction1 = RoadUtil.SelectJunctionForCell(endPoint8K, job.inOutTerrain8KM.cellSize);
                        }

                        Bounds2D searchBounds = BNavigation.CreateSearchBounds(new NavigationPathSearchBounds
                        {
                            startCell = junction0,
                            finishCell = junction1,
                            cell8KMSize = job.inOutTerrain8KM.cellSize,
                            include8KMWorldPoints = km8Points.AsArray(),
                            terrainSize = terrainSize
                        });
                        
                        resultSegments.Add(new RoadSegment
                        {
                            point0 = junction0,
                            point1 = junction1,
                            bounds = searchBounds
                        });
                        
                        pointIndex0 = pointIndex1;
                        junction0 = junction1;
                    }
                    
                    pointIndex1++;
                }
            }

            return resultSegments;
        }

        private void _debugDrawPoints8KM(NativeList<ushort2> points)
        {
            int cellSize8KM = _biome.Terrain8kmCell.cellSize;

            _gizmo.SetColor(Color.black.WithAlpha(0.8f));

            HashSet<ushort2> addedPoints = new(points.Length);

            foreach (ushort2 position in points)
            {
                if (addedPoints.Add(position))
                {
                    float2 cellCenter = _biome.Terrain8kmCell.GetCellCenterWorld(position);
                    
                    _gizmo.AddQuad(cellCenter, cellSize8KM);
                }
            }
        }

        [Conditional("DEVELOPMENT")]
        private void _debugDrawRoads(NativeSlice<RoadInfo> roads)
        {
            HashSet<ushort2> junctionPoints = new (1024);
            
            foreach (RoadInfo road in roads)
            {
                _gizmo.SetColor(Color.red.WithAlpha(0.8f));
                
                if (junctionPoints.Add(road.startPoint))
                {
                    _gizmo.AddPoint(road.startPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                if (junctionPoints.Add(road.endPoint))
                {
                    _gizmo.AddPoint(road.endPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                _gizmo.SetColor(Color.black);

                for (int i = 0; i < road.points.Length; i++)
                {
                    _gizmo.AddPoint(road.points[i] + new float2(0.5f, 0.5f), 2f);
                }
            }
        }

        protected override void _dispose(BlockingContext obj)
        {
            _gizmo.Dispose();
            
            if (_roads.IsCreated)
            {
                _roads.Dispose();
                _roadPoints.Dispose();
            }
        }
    }
}