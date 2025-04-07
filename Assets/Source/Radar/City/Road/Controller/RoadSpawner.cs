using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
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

using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.City
{
    public class RoadSpawner : Actor, IRoadSpawner
    {
        public RoadPathMap RoadMap => _roadMap;
        
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly ITerrainBackedBiome _biome;
        [Required] private readonly ICitySpawner _citySpawner;
        [Required] private readonly RoadSpawnConfig _config;
        [Required] private readonly IGizmoManager _gizmoManager;
        
        private TerrainData _bakedForTerrain;
        private IGizmoHandle _gizmo;

        private RoadPathMap _roadMap;

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
                BProfiler.BeginSample("ReadRoadCache");
                
                using IBinaryReader reader = _worldManager.StartCacheReading(world.guid, FileName.ROAD_MAP);
            
                _roadMap.Deserialize(reader);
                
                BProfiler.EndSample();
            }
            else
            {
                _config.buildCost.ToByteArray(out ByteArray16 cellBiomeCost, out ByteArray16 cellFeatureCost);

                AveragePixelCountTexture<TerrainCell> terrain8KMClone = _biome.Terrain8kmCell.Clone(Allocator.TempJob);

                RoadPath8KMJob roadPath8KMJob = new()
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

                roadPath8KMJob.Invoke();

                terrain8KMClone.Dispose();

                int maxSegmentSize8KM = 
                    BMath.CeilToInt(_config.maxSegmentLength / (float)_biome.Terrain8kmCell.cellSize);

                BProfiler.BeginSample("RoadUtil.CreateRoadSegments");
                using NativeList<RoadSegment> segmentsToSearch = RoadUtil.CreateRoadSegments(
                    roadPath8KMJob,
                    _biome.Terrain1kmCell.TextureSize,
                    maxSegmentSize8KM,
                    Allocator.TempJob);
                BProfiler.EndSample();

                int maxPointPerSegment = BMath.CeilToInt(maxSegmentSize8KM * _biome.Terrain8kmCell.cellSize * 2f);

                RoadPath1KMJob roadPath1KMJob = new()
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

                roadPath1KMJob.Invoke(segmentsToSearch.Length, 8);

                BProfiler.BeginSample("RoadUtil.CreateRoadPathMap");
                _roadMap = RoadUtil.CreateRoadPathMap(roadPath1KMJob, maxPointPerSegment, Allocator.Persistent);
                BProfiler.EndSample();

                using IBinaryWriter writer = _worldManager.StartCacheWriting(world.guid, FileName.ROAD_MAP);
                _roadMap.Serialize(writer);
                
                roadPath8KMJob.outPoints.Dispose();
                roadPath8KMJob.outJunctions.Dispose();
                roadPath8KMJob.outSegments.Dispose();

                roadPath1KMJob.outPoints.Dispose();
                roadPath1KMJob.outSegmentPointCount.Dispose();
                roadPath1KMJob.outSegments.Dispose();
            }
            
            RoadCellFeatureJob featureJob = new()
            {
                inRoadMap = _roadMap,
                inOutTerrain1KM = _biome.Terrain1kmCell,
                inOutTerrain8KM = _biome.Terrain8kmCell
            };

            featureJob.Invoke(_roadMap.roads.Length, 8);

            BDebug.Track(this, "RoadCount", _roadMap.roads.Length);
            BDebug.Track(this, "PointCount", _roadMap.roadPoints.Length);

            // RoadDebug.DrawRoadPathMap(_gizmo, _roadMap);
            
            return UniTask.CompletedTask;
        }
        
        protected override void _dispose(BlockingContext obj)
        {
            _gizmo.Dispose();
            _roadMap.Dispose();
        }
    }
}