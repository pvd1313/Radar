using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmo
{
    public class GizmoHandle : IGizmoHandle
    {
        private const int POINT_PIXEL_SIZE = 4;
        
        public bool IsDisposed { get; private set; }
        
        public NativeArray<ushort> buffer;
        public int shapeCount;

        private ushort _colorX;
        private ushort _colorY;

        public GizmoHandle()
        {
            _ensureCapacity(128);
            
            BConvert.ColorToHalf2(Color.white, out _colorX, out _colorY);
        }

        public void SetColor(Color color)
        {
            BConvert.ColorToHalf2(color, out _colorX, out _colorY);
        }

        public void Clear()
        {
            shapeCount = 0;
        }

        public void AddPoint(float2 position)
        {
            AddPoint(position, POINT_PIXEL_SIZE);
        }
        
        public void AddPoint(float2 position, float pixelSize)
        {
            _ensureCapacity(shapeCount + 1);
            
            int offset = shapeCount * GizmoUtil.FLOAT_PER_SHAPE;
            BConvert.FloatToHalf2(position.x, out ushort px0, out ushort px1);
            BConvert.FloatToHalf2(position.y, out ushort py0, out ushort py1);
            
            buffer[offset + 0] = BConvert.FloatToHalf(0);
            buffer[offset + 1] = BConvert.FloatToHalf(pixelSize);
            buffer[offset + 2] = _colorX;
            buffer[offset + 3] = _colorY;
            buffer[offset + 4] = px0;
            buffer[offset + 5] = px1;
            buffer[offset + 6] = py0;
            buffer[offset + 7] = py1;

            shapeCount++;
        }

        public void AddQuad(float2 position, float worldSize)
        {
            _ensureCapacity(shapeCount + 1);
            
            int offset = shapeCount * GizmoUtil.FLOAT_PER_SHAPE;
            BConvert.FloatToHalf2(position.x, out ushort px0, out ushort px1);
            BConvert.FloatToHalf2(position.y, out ushort py0, out ushort py1);
            
            buffer[offset + 0] = BConvert.FloatToHalf(1);
            buffer[offset + 1] = BConvert.FloatToHalf(worldSize);
            buffer[offset + 2] = _colorX;
            buffer[offset + 3] = _colorY;
            buffer[offset + 4] = px0;
            buffer[offset + 5] = px1;
            buffer[offset + 6] = py0;
            buffer[offset + 7] = py1;

            shapeCount++;
        }

        private void _ensureCapacity(int targetCapacity)
        {
            if (buffer.IsCreated && buffer.Length > targetCapacity * GizmoUtil.FLOAT_PER_SHAPE)
            {
                return;
            }

            int newCapacity = BMath.CeilToInt(targetCapacity * GizmoUtil.FLOAT_PER_SHAPE / 128.0f) * 128;
            NativeArray<ushort> newBuffer = Native.PermArray<ushort>(newCapacity);

            if (buffer.IsCreated)
            {
                newBuffer.CopyFromDifferenceSize(buffer);
                buffer.Dispose();
            }

            buffer = newBuffer;
        }

        public void Dispose()
        {
            IsDisposed = true;
            
            buffer.Dispose();
        }
    }
}