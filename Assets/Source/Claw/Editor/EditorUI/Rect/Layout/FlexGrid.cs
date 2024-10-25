using System;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public ref struct FlexGrid
    {
        public ref struct Input
        {
            public float2 screenSize;
            public Span<float2> elementSizes;
            public Span<float2> outputPositions;
        }

        public float2 TotalSize => _totalSize;

        private Span<float2> _sizes;
        private Span<float2> _positions;
        private float2 _totalSize;

        public FlexGrid(Input input)
        {
            if (input.outputPositions.Length < input.elementSizes.Length)
            {
                throw new InvalidOperationException("outputPositions capacity is not enough");
            }

            _positions = input.outputPositions;
            _sizes = input.elementSizes;
            _totalSize.x = input.screenSize.x;
            float localMaxY = 0;
            float yOffset = 0;
            float xOffset = 0;

            for (int i = 0; i < input.elementSizes.Length; i++)
            {
                float2 elementSize = input.elementSizes[i];

                if (xOffset + elementSize.x > input.screenSize.x)
                {
                    xOffset = elementSize.x;
                    yOffset += localMaxY;
                    
                    _positions[i] = new float2(0, yOffset);

                    localMaxY = elementSize.y;
                }
                else
                {
                    _positions[i] = new float2(xOffset, yOffset);
                    
                    xOffset += elementSize.x;
                    localMaxY = BMath.Max(localMaxY, elementSize.y);
                }
            }

            _totalSize.y = yOffset + localMaxY;
        }
        
        public Rect this[int index]
        {
            get
            {
                if (index < 0 || index > _positions.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return new Rect(_positions[index], _sizes[index]);
            }
        }
    }
}