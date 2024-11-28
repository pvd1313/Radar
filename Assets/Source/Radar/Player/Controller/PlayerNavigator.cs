using System.Diagnostics;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Beast.Radar.Graphic;
using Beast.Radar.Navigation;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Player
{
    public class PlayerNavigator : Controller
    {
        private const float MAX_PATH_DISTANCE_1KM = 128;
        
        private readonly IGizmoManager _gizmoManager;
        private readonly PlayerConfig _config;
        private readonly ITerrainBackedBiome _biome;

        private PathJobBuffer _pathJobBuffer;
        private Stopwatch _stopwatch;

        private IGizmoHandle _gizmo8KM;
        private PathNavigator _path8KM;

        private IGizmoHandle _gizmo1KM;
        private PathNavigator _path1KM;

        private NativeList<float2> _consumed8KMPoints;

        private float2 _finishPosition;

        public PlayerNavigator(IGizmoManager gizmoManager, PlayerConfig config, ITerrainBackedBiome biome)
        {
            _gizmoManager = gizmoManager;
            _config = config;
            _biome = biome;
        }
        
        public void Warmup()
        {
            _pathJobBuffer = PathJobBuffer.Create(_config.pathBufferCapacity, Allocator.Persistent);
            
            _path8KM = new PathNavigator(_config.pathBufferCapacity);
            _gizmo8KM = _gizmoManager.CreateHandle(this);

            _path1KM = new PathNavigator(_config.pathBufferCapacity);
            _gizmo1KM = _gizmoManager.CreateHandle(this);

            _consumed8KMPoints = BNative.PermList<float2>(16);
            
            _stopwatch = new Stopwatch();
        }

        public void MovePlayer(ref float2 playerPosition, float movementDelta, out bool isDestinationReached)
        {
            isDestinationReached = false;

            if (BMath.Distance(_finishPosition, playerPosition) < BWorld.DESTINATION_REACHED_DISTANCE)
            {
                isDestinationReached = true;
                return;
            }

            float2 targetPosition = _finishPosition;

            _consume8kmAndRebuild1km(playerPosition, false);

            if (_path1KM.GetRemainingNodeCount() > 0)
            {
                targetPosition = _path1KM.GetTargetNodeWorldPosition();
            }

            playerPosition = BMath.MoveTowards(playerPosition, targetPosition, BMath.Length(movementDelta));

            if (_path1KM.GetRemainingNodeCount() > 0)
            {
                if (_path1KM.GetRemainingNodeCount() == 1 ||
                    BMath.Distance(playerPosition, targetPosition) < 1)
                {
                    _path1KM.SwitchToNextTargetNode();
                }
            }
        }

        public void Setup(float2 start, float2 finish)
        {
            _finishPosition = finish;
            
            _buildPath8KM(start, finish);
            
            _consumed8KMPoints.Clear();

            _consume8kmAndRebuild1km(start, true);
        }

        public void Clear()
        {
            _gizmo1KM.Clear();
            _gizmo8KM.Clear();
            _path1KM.Clear();
            _path8KM.Clear();
        }

        private void _consume8kmAndRebuild1km(float2 playerPosition, bool isForce)
        {
            bool mustRebuild1KM = isForce;
            
            float2 targetPosition = _finishPosition;

            bool isFirstConsume = true;
            
            while (_path8KM.GetRemainingNodeCount() > 0)
            {
                float2 position8KM = _path8KM.GetTargetNodeWorldPosition();

                if (BMath.Distance(position8KM, playerPosition) > MAX_PATH_DISTANCE_1KM)
                {
                    break;
                }

                targetPosition = position8KM;
                mustRebuild1KM = true;

                if (isFirstConsume)
                {
                    if (_consumed8KMPoints.Length > 0)
                    {
                        _consumed8KMPoints.RemoveAt(0);
                    }
                    
                    isFirstConsume = false;
                }
                
                _consumed8KMPoints.Add(position8KM);
                
                _path8KM.SwitchToNextTargetNode();

                if (_path8KM.GetRemainingNodeCount() == 0)
                {
                    targetPosition = _finishPosition;
                }
            }

            if (mustRebuild1KM)
            {
                _buildPath1KM(playerPosition, targetPosition);
            }
        }

        private void _buildPath8KM(float2 start, float2 finish)
        {
            int textureSize = _biome.Terrain8kmCell.cellCountX;
            
            FindPathJob<AverageBiomeTexture> job = new ()
            {
                startPosition = _biome.Terrain8kmCell.WorldPositionToCell(start),
                endPosition = _biome.Terrain8kmCell.WorldPositionToCell(finish),
                tileCost = _config.TileSpeedModifiersToTileCost(),
                texture = new AverageBiomeTexture(_biome.Terrain8kmCell),
                buffer = _pathJobBuffer,
                searchBounds = new Bounds2D(0, textureSize - 1, 0, textureSize - 1)
            };
            
            _stopwatch.Restart();
            Profiler.BeginSample("8km_job");
            job.Schedule().Complete();
            Profiler.EndSample();
            _stopwatch.Stop();
            BDebug.Track(this, "8km_job_ms", _stopwatch.ElapsedMillisecondsFloat());

            _path8KM.Setup(job.buffer.outputPath, _biome.Terrain8kmCell.cellSize);
            _path8KM.DrawGizmo(_gizmo8KM, Color.magenta.WithAlpha(0.5f));
        }

        private void _buildPath1KM(float2 start, float2 finish)
        {
            int2 startCell = _biome.Terrain1kmCell.WorldPositionToCell(start);
            int2 finishCell = _biome.Terrain1kmCell.WorldPositionToCell(finish);

            Bounds2D searchBounds = Bounds2D.FromTwoPoints(startCell, finishCell);

            float searchRangeX = searchBounds.x.GetRange();
            float searchRangeY = searchBounds.y.GetRange();

            float deltaRangeHalf = BMath.Ceil((searchRangeY - searchRangeX) * 0.5f);

            // This will make search bounds to be a square.
            if (deltaRangeHalf > 0)
            {
                searchBounds.x = searchBounds.x.Expand(deltaRangeHalf);
            }
            else
            {
                searchBounds.y = searchBounds.y.Expand(deltaRangeHalf * -1);
            }

            float km8Radius = _biome.Terrain8kmCell.cellSize * 0.5f;
            foreach (float2 point8KM in _consumed8KMPoints)
            {
                Bounds2D pointBounds = Bounds2D.FromPointWithRadius(point8KM, km8Radius);
                searchBounds = searchBounds.EnsureContains(pointBounds);
            }
            
            int textureSize = _biome.Terrain1kmCell.blockSize * _biome.Terrain1kmCell.blockCountX;
            Bounds2D textureBounds = new (0, textureSize - 1);

            searchBounds = searchBounds.EnsureInside(textureBounds);
            
            FindPathJob<BiomeTextureWithBlocks> job = new()
            {
                startPosition = startCell,
                endPosition = finishCell,
                tileCost = _config.TileSpeedModifiersToTileCost(),
                texture = new BiomeTextureWithBlocks(_biome.Terrain1kmCell),
                buffer = _pathJobBuffer,
                searchBounds = searchBounds,
            };
            
            _stopwatch.Restart();
            Profiler.BeginSample("1km_job");
            job.Schedule().Complete();
            Profiler.EndSample();
            _stopwatch.Stop();
            BDebug.Track(this, "1km_job_ms", _stopwatch.ElapsedMillisecondsFloat());
            
            _path1KM.Setup(job.buffer.outputPath, 1);
            _path1KM.DrawGizmo(_gizmo1KM, Color.yellow.WithAlpha(0.5f));
        }

        public float GetRemainingDistance()
        {
            return _path8KM.GetDistanceFromTargetNodeToFinish() + _path1KM.GetDistanceFromTargetNodeToFinish();
        }

        public void Dispose()
        {
            _gizmo1KM.Dispose();
            _gizmo8KM.Dispose();
            _pathJobBuffer.Dispose();
            _path1KM.Dispose();
            _path8KM.Dispose();
            _consumed8KMPoints.Dispose();
        }
    }
}