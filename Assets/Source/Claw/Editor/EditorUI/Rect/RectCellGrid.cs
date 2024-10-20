using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public readonly struct RectCellGrid
    {
        public struct Input
        {
            public float2 windowSize;
            public float cellSize;
            public float spacing;
            public int elementCount;
            public float padding;
        }

        public float2 TotalSize => _totalSize;

        private readonly int _columnCount;
        private readonly float2 _totalSize;
        private readonly float _cellSize;
        private readonly float2 _spacing;
        private readonly float _padding;
        
        public RectCellGrid(Input input)
        {
            _padding = input.padding;
            
            float windowWidth = input.windowSize.x - _padding * 2;
            
            _cellSize = input.cellSize;
            _columnCount = BMath.FloorToInt(windowWidth / _cellSize);
            _spacing = input.spacing;
            
            _columnCount = BMath.Max(1, _columnCount);

            // _columnCount = BMath.Min(_columnCount, input.elementCount);
            
            if (_columnCount * _cellSize + (_columnCount - 1) * _spacing.x > windowWidth)
            {
                _columnCount = BMath.Max(1, _columnCount - 1);
            }
            
            if (_columnCount > 1 && input.elementCount >= _columnCount)
            {
                _spacing.x = (windowWidth - _cellSize * _columnCount) / (_columnCount - 1);
            }
            
            // Remove horizontal slider.
            _totalSize.x = windowWidth - 20;
            int rowCount = BMath.CeilToInt(input.elementCount / (float)_columnCount);
            _totalSize.y = _cellSize * rowCount + (rowCount - 1) * _spacing.y;
        }

        public Rect GetElement(int elementIndex)
        {
            Vector2 position;
            
            position.x = _getOffset(elementIndex % _columnCount, _cellSize, _spacing.x) + _padding;
            position.y = _getOffset(elementIndex / _columnCount, _cellSize, _spacing.y) + _padding;

            return new Rect(position, new Vector2(_cellSize, _cellSize));
        }

        private static float _getOffset(int x, float elementSize, float spacing)
        {
            return x * elementSize + x * spacing;
        }
    }
}