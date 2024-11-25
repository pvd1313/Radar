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

namespace Beast.Radar.Player
{
    public class PlayerNavigator : Controller
    {
        public float RemainingDistance => _remainingDistance;
        
        private readonly IGizmoManager _gizmoManager;
        private readonly PlayerConfig _config;
        private readonly ITerrainBackedBiome _biome;

        private IGizmoHandle _gizmo;
        private PathJobBuffer _pathJobBuffer;
        private Stopwatch _stopwatch;

        private NativeList<int2> _path;
        private int _pathRemainingDiagonalCount;
        private int _pathRemainingStraightCount;
        private float _remainingDistance;

        private int _targetNodeIndex;
        
        public PlayerNavigator(
            IGizmoManager gizmoManager, 
            PlayerConfig config, 
            ITerrainBackedBiome biome)
        {
            _gizmoManager = gizmoManager;
            _config = config;
            _biome = biome;
        }

        public void Warmup()
        {
            _gizmo = _gizmoManager.CreateHandle(this);
            
            _pathJobBuffer = new PathJobBuffer
            {
                openList = new PriorityQueue(Allocator.Persistent),
                outputPath = new NativeList<int2>(Allocator.Persistent),
                pathNodeList = new NativeList<PathNode>(_config.maxNodeCount, Allocator.Persistent),
                positionToIndexMap = new NativeHashMap<int2, int>(_config.maxNodeCount, Allocator.Persistent),
                closedList = new NativeHashSet<int>(_config.maxNodeCount, Allocator.Persistent),
                openSet = new NativeHashSet<int>(_config.maxNodeCount, Allocator.Persistent),
            };

            _path = BNative.PermList<int2>(256);
            _targetNodeIndex = 0;

            _stopwatch = new Stopwatch();
        }

        public void MovePlayer(ref float2 position, float movementDelta, out bool isDestinationReached)
        {
            isDestinationReached = false;
            
            if (_targetNodeIndex >= _path.Length)
            {
                isDestinationReached = true;
                return;
            }

            int2 targetNode = _path[_targetNodeIndex];
            float2 targetNodeWorld = _getCellCenter(targetNode);
            
            position = BMath.MoveTowards(position, targetNodeWorld, BMath.Length(movementDelta));

            if (BMath.Distance(position, targetNodeWorld) < BWorld.DESTINATION_REACHED_DISTANCE)
            {
                if (_targetNodeIndex > 0)
                {
                    if (BNavigation.IsStraight(_path[_targetNodeIndex - 1], targetNode))
                    {
                        _pathRemainingStraightCount--;
                    }
                    else
                    {
                        _pathRemainingDiagonalCount--;
                    }
                }
                
                _targetNodeIndex++;
            }

            _remainingDistance = _calcRemainingDistance(position);

            BDebug.Track(this, nameof(_targetNodeIndex), _targetNodeIndex);
        }

        private float _calcRemainingDistance(float2 playerPosition)
        {
            int straightCount = _pathRemainingStraightCount;
            int diagonalCount = _pathRemainingDiagonalCount;

            float distance = 0;

            if (_targetNodeIndex > 0 && _targetNodeIndex < _path.Length)
            {
                int2 fromNode = _path[_targetNodeIndex - 1];
                int2 targetNode = _path[_targetNodeIndex];

                if (BNavigation.IsStraight(fromNode, targetNode))
                {
                    straightCount--;
                }
                else
                {
                    diagonalCount--;
                }

                distance += BMath.Distance(playerPosition, _getCellCenter(targetNode));
            }

            distance += _biome.Terrain8kmCell.cellSize * straightCount;
            distance += diagonalCount * _biome.Terrain8kmCell.cellSize * BMath.SQRT2;

            return distance;
        }

        private float2 _getCellCenter(int2 cellIndex)
        {
            float2 cellSizeHalf = _biome.Terrain8kmCell.cellSize * 0.5f;
            
            return _biome.Terrain8kmCell.CellToWorldPoint(cellIndex) + cellSizeHalf;
        }

        public void SetupPath(float2 start, float2 finish)
        {
            FindPathJob job = new ()
            {
                startPosition = _biome.Terrain8kmCell.WorldPositionToCell(start),
                endPosition = _biome.Terrain8kmCell.WorldPositionToCell(finish),
                tileCosts = _config.TileSpeedModifiersToTileCost(),
                textureSizeX = _biome.Terrain8kmCell.cellCountX,
                texture = _biome.Terrain8kmCell.texture,
                outputReversedPath = _path,
                jobBuffer = _pathJobBuffer
            };
            
            _stopwatch.Restart();
            
            job.Schedule().Complete();
            
            _stopwatch.Stop();
            
            _path.Reverse();
            _targetNodeIndex = 0;
            BNavigation.CalcPathNodeTypeCount(_path, out _pathRemainingDiagonalCount, out _pathRemainingStraightCount);

            _remainingDistance = _calcRemainingDistance(start);
            
            BDebug.Track(this, "jobElapsedMS", _stopwatch.ElapsedMillisecondsFloat());
            BDebug.Track(this, "nodeCount", _path.Length);
            BDebug.Track(this, nameof(_targetNodeIndex), _targetNodeIndex);
            
            _redrawPathGizmo();
        }
        
        [Conditional("DEVELOPMENT")]
        private void _redrawPathGizmo()
        {
            _gizmo.Clear();
            _gizmo.SetColor(Color.magenta.WithAlpha(0.4f));
            
            float cellSize = _biome.Terrain8kmCell.cellSize;
            float2 cellSizeHalf = cellSize * 0.5f;
            
            if (_path.Length > 0)
            {
                foreach (int2 point in _path)
                {
                    float2 position = _biome.Terrain8kmCell.CellToWorldPoint(point) + cellSizeHalf;

                    _gizmo.AddQuad(position, cellSize);
                }
            }
        }

        public void Dispose()
        {
            _gizmo.Dispose();
            _pathJobBuffer.Dispose();
            _path.Dispose();
        }
    }
}