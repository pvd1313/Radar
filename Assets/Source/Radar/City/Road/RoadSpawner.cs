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
using Beast.Claw.Serialization;
using Beast.Radar.Navigation;
using Beast.Radar.Serialization;
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
        public RoadMap RoadMap => _roadMap;
        
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly RoadSpawnConfig _config;
        [Required] private readonly IGizmoManager _gizmoManager;
        
        private TerrainData _bakedForTerrain;
        private IGizmoHandle _gizmo;

        private RoadMap _roadMap;

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            return UniTask.CompletedTask;
        }

        protected override UniTask _gameLoad(ConcurrentContext context)
        {
            WorldData world = _worldManager.GetCurrentWorld();
            TerrainData terrain = world.terrain;

            if (terrain.IsSame(_bakedForTerrain))
            {
                return UniTask.CompletedTask;
            }

            _bakedForTerrain = terrain;
            
            _roadMap.Dispose();

            bool roadMapCacheExists = _worldManager.CacheExists(world.guid, FileName.ROAD_MAP);

            if (roadMapCacheExists)
            {
                Profiler.BeginSample("ReadRoadCache");
                
                using IBinaryReader reader = _worldManager.OpenCacheForReading(world.guid, FileName.ROAD_MAP);

                _roadMap.Deserialize(reader);
                
                Profiler.EndSample();
            }
            else
            {
                _config.buildCost.ToByteArray(out ByteArray16 cellBiomeCost, out ByteArray16 cellFeatureCost);

                AveragePixelCountTexture<TerrainCell> terrain8KMClone = _biome.Terrain8kmCell.Clone(Allocator.TempJob);

                Road8KMJob road8KMJob = new()
                {
                    inCellBiomeCost = cellBiomeCost,
                    inCellFeatureCost = cellFeatureCost,
                    inCities = _citySpawner.Cities,
                    inOutTerrain8KM = terrain8KMClone,
                    inSpawnPriority = _config.spawnPriority,
                    outPoints = BNative.TempJobList<ushort2>(2048),
                    outJunctions = new NativeHashMap<ushort2, ushort2>(256, Allocator.TempJob),
                    outSegments = BNative.TempJobList<ushort>(256),
                };

                Profiler.BeginSample("RoadSegment8KMJob");
                road8KMJob.Run();
                Profiler.EndSample();

                terrain8KMClone.Dispose();

                int maxSegmentSize8KM =
                    BMath.CeilToInt(_config.maxSegmentLength / (float)_biome.Terrain8kmCell.cellSize);

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

                NativeArray<ushort2> roadPoints = BNative.PermArray<ushort2>(totalPointCount);
                NativeArray<RoadInfo> roads = BNative.PermArray<RoadInfo>(road1KMJob.outSegments.Length);

                int writeOffset = 0;
                int readOffset = 0;
                for (int i = 0; i < road1KMJob.outSegments.Length; i++)
                {
                    RoadSegment segment = road1KMJob.outSegments[i];
                    int segmentPointCount = road1KMJob.outSegmentPointCount[i];

                    NativeSlice<ushort2> target = roadPoints.Slice(writeOffset, segmentPointCount);
                    NativeSlice<ushort2> source = road1KMJob.outPoints.Slice(readOffset, segmentPointCount);

                    target.CopyFrom(source);

                    roads[i] = new RoadInfo
                    {
                        startPointIndex = (ushort)writeOffset,
                        pointCount = (ushort)segmentPointCount,
                        bounds = segment.bounds,
                    };

                    writeOffset += segmentPointCount;
                    readOffset += maxPointPerSegment;
                }

                Profiler.EndSample();

                _roadMap = new RoadMap
                {
                    roads = roads,
                    roadPoints = roadPoints
                };

                using IBinaryWriter writer = _worldManager.OpenCacheForWriting(world.guid, FileName.ROAD_MAP);
                _roadMap.Serialize(writer);
                
                road8KMJob.outPoints.Dispose();
                road8KMJob.outJunctions.Dispose();
                road8KMJob.outSegments.Dispose();
                segmentsToSearch.Dispose();

                road1KMJob.outPoints.Dispose();
                road1KMJob.outSegmentPointCount.Dispose();
                road1KMJob.outSegments.Dispose();
            }
            
            Profiler.BeginSample("RoadCellFeatureSetJob");
            RoadCellFeatureSetJob featureSetJob = new()
            {
                inRoadMap = _roadMap,
                inOutTerrain1KM = _biome.Terrain1kmCell,
                inOutTerrain8KM = _biome.Terrain8kmCell
            };

            featureSetJob.Schedule(_roadMap.roads.Length, 8).Complete();
            Profiler.EndSample();

            BDebug.Track(this, "RoadCount", _roadMap.roads.Length);
            BDebug.Track(this, "PointCount", _roadMap.roadPoints.Length);

            _gizmo.Clear();

            _debugDrawRoads();
            
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

        [Conditional("DEVELOPMENT")]
        private void _debugDrawRoads()
        {
            HashSet<ushort2> junctionPoints = new (1024);
            
            foreach (RoadInfo road in _roadMap.roads)
            {
                _gizmo.SetColor(Color.red.WithAlpha(0.8f));

                ushort2 startPoint = _roadMap.roadPoints[road.startPointIndex];
                ushort2 endPoint = _roadMap.roadPoints[road.startPointIndex + road.pointCount - 1];
                
                if (junctionPoints.Add(startPoint))
                {
                    _gizmo.AddPoint(startPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                if (junctionPoints.Add(endPoint))
                {
                    _gizmo.AddPoint(endPoint + new float2(0.5f, 0.5f), 8f);
                }
                
                _gizmo.SetColor(Color.black);

                for (int i = 1; i < road.pointCount - 1; i++)
                {
                    ushort2 point = _roadMap.roadPoints[road.startPointIndex + i];
                    
                    _gizmo.AddPoint(point + new float2(0.5f, 0.5f), 2f);
                }
            }
        }

        protected override void _dispose(BlockingContext obj)
        {
            _gizmo.Dispose();
            _roadMap.Dispose();
        }
    }
}