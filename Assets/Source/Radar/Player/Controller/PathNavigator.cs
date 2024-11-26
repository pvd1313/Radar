using System;
using System.Diagnostics;
using Beast.Claw.Common;
using Beast.Claw.Gizmo;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Player
{
    public struct PathNavigator
    {
        private NativeList<int2> _path;
        private int _targetNodeIndex;
        private int _diagonalNodeLeft;
        private int _straightNodeLeft;
        private float _cellSize;

        public PathNavigator(int capacity)
        {
            _path = BNative.PermList<int2>(capacity);
            _targetNodeIndex = -1;
            _diagonalNodeLeft = 0;
            _straightNodeLeft = 0;
            _cellSize = 0;
        }

        public void Setup(NativeList<int2> reversedPath, float cellSize)
        {
            _path.Clear();
            _path.AddRangeReverseOrder(reversedPath);

            _targetNodeIndex = 0;
            _cellSize = cellSize;
            
            BNavigation.CalcPathNodeTypeCount(_path, out _diagonalNodeLeft, out _straightNodeLeft);
        }

        [Conditional("DEVELOPMENT")]
        public void DrawGizmo(IGizmoHandle gizmo, Color color)
        {
            gizmo.Clear();
            gizmo.SetColor(color);

            float2 cellSizeHalf = _cellSize * 0.5f;

            if (_path.Length > 0)
            {
                foreach (int2 point in _path)
                {
                    float2 position = point.Mul(_cellSize) + cellSizeHalf;
                    
                    gizmo.AddQuad(position, _cellSize);
                }
            }
        }

        public void Clear()
        {
            _path.Clear();
            _targetNodeIndex = 0;
        }

        public float2 GetTargetNodeWorldPosition()
        {
            if (_targetNodeIndex >= _path.Length)
            {
                throw new InvalidOperationException("No nodes left.");
            }

            int2 cellPosition = _path[_targetNodeIndex];
            float2 cellSizeHalf = _cellSize * 0.5f;

            return cellPosition.Mul(_cellSize) + cellSizeHalf;
        }

        public int GetRemainingNodeCount()
        {
            return _path.Length - _targetNodeIndex;
        }

        public void SwitchToNextTargetNode()
        {
            if (_targetNodeIndex >= _path.Length)
            {
                throw new InvalidOperationException("No nodes left.");
            }

            if (_targetNodeIndex > 0)
            {
                int2 a = _path[_targetNodeIndex - 1];
                int2 b = _path[_targetNodeIndex];
                
                if (BNavigation.IsStraight(a, b))
                {
                    _straightNodeLeft--;
                }
                else
                {
                    _diagonalNodeLeft--;
                }
            }

            _targetNodeIndex++;
        }

        public float GetDistanceFromTargetNodeToFinish()
        {
            return _cellSize * _straightNodeLeft + _cellSize * _diagonalNodeLeft * BMath.SQRT2;
        }

        public void Dispose()
        {
            if (_path.IsCreated)
            {
                _path.Dispose();
            }
        }
    }
}