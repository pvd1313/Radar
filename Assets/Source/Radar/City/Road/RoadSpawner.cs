using System.Collections.Generic;
using System.Diagnostics;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
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
    public class RoadSpawner : Actor
    {
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly RoadSpawnConfig _config;
        [Required] private readonly IGizmoManager _gizmoManager;
        
        private TerrainData _bakedForTerrain;
        private IGizmoHandle _gizmo;

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
            
            RoadSegment8KMJob road8KmJob = new()
            {
                inCities = _citySpawner.Cities,
                inOutTerrain8KM = _biome.Terrain8kmCell,
                inSpawnPriority = _config.spawnPriority,
                outPoints = BNative.TempJobList<ushort2>(2048),
                outJunctions = new NativeHashMap<ushort2, ushort2>(256, Allocator.TempJob),
                outSegments = BNative.TempJobList<ushort>(256),
            };
            
            _config.buildCost.ToByteArray(out road8KmJob.inCellBiomeCost, out road8KmJob.inCellFeatureCost);
            
            Profiler.BeginSample("RoadSegment8KMJob");
            road8KmJob.Run();
            Profiler.EndSample();

            NativeList<RoadSegment> segments = _collectRoadSegments(road8KmJob);
            
            _gizmo.Clear();

            _debugDrawPoints8KM(road8KmJob.outPoints);

            _debugDrawSegments(segments);

            road8KmJob.outPoints.Dispose();
            road8KmJob.outJunctions.Dispose();
            road8KmJob.outSegments.Dispose();
            segments.Dispose();
            
            return UniTask.CompletedTask;
        }
        
        private NativeList<RoadSegment> _collectRoadSegments(RoadSegment8KMJob job)
        {
            NativeList<RoadSegment> resultSegments = BNative.TempJobList<RoadSegment>(256);
            
            int maxSegSize = BMath.CeilToInt(_config.maxSegmentLength / (float)_biome.Terrain8kmCell.cellSize);

            BDebug.Track(this, "MaxSegmentSize", maxSegSize);
            
            int pointOffset = 0;
            
            for (int segID = 0; segID < job.outSegments.Length; segID++)
            {
                int segPointCount = job.outSegments[segID];
                
                int pointIndex0 = pointOffset;
                ushort2 junction0 = job.outJunctions[job.outPoints[pointIndex0]];
                
                int pointIndex1 = pointIndex0 + 1;
                
                pointOffset += segPointCount;

                while (pointIndex0 < pointOffset - 1)
                {
                    int pointCount = pointIndex1 - pointIndex0;

                    ushort2 endPoint8K = job.outPoints[pointIndex1];
                    
                    bool endIsJunction = job.outJunctions.TryGetValue(endPoint8K, out ushort2 junction1);

                    if (pointCount >= BMath.Min(maxSegSize, segPointCount) || endIsJunction)
                    {
                        if (!endIsJunction)
                        {
                            junction1 = RoadUtil.SelectJunctionForCell(endPoint8K, job.inOutTerrain8KM.cellSize);
                        }
                        
                        resultSegments.Add(new RoadSegment
                        {
                            point0 = junction0,
                            point1 = junction1
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
                    _gizmo.AddQuad((float2)position * cellSize8KM + cellSize8KM * 0.5f, cellSize8KM);
                }
            }
        }

        [Conditional("DEVELOPMENT")]
        private void _debugDrawSegments(NativeList<RoadSegment> segments)
        {
            HashSet<ushort2> drawnPoints = new (1024);
            
            foreach (RoadSegment roadSegment in segments)
            {
                _gizmo.SetColor(Color.red.WithAlpha(0.8f));
                
                float2 p0 = roadSegment.point0 + new float2(0.5f, 0.5f);
                float2 p1 = roadSegment.point1 + new float2(0.5f, 0.5f);
                
                if (drawnPoints.Add(roadSegment.point0))
                {
                    _gizmo.AddPoint(p0, 8f);
                }
                
                if (drawnPoints.Add(roadSegment.point1))
                {
                    _gizmo.AddPoint(p1, 8f);
                }
                
                _gizmo.SetColor(Color.green.WithAlpha(0.8f));

                for (int i = 1; i < 10; i++)
                {
                    float t = i / 10.0f;
                    float2 pos = BMath.Lerp(p0, p1, t);

                    _gizmo.AddPoint(pos, 4f);
                }
            }
        }

        protected override void _dispose(BlockingContext obj)
        {
            _gizmo.Dispose();
        }
    }
}