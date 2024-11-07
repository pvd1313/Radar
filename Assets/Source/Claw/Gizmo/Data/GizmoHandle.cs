using Beast.Claw.Collection;
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
        
        public NativeArray<float> buffer;
        public int shapeCount;

        private float _color;

        public GizmoHandle()
        {
            _ensureCapacity(128);
            _color = BGraphic.RGBA8ToFloat(Color.white);
        }

        public void SetColor(Color color)
        {
            _color = BGraphic.RGBA8ToFloat(color);
        }

        public void Clear()
        {
            shapeCount = 0;
        }

        public void AddPoint(float2 position)
        {
            _ensureCapacity(shapeCount + 1);

            int offset = shapeCount * GizmoUtil.FLOAT_PER_SHAPE;
            buffer[offset + 0] = 0;
            buffer[offset + 1] = position.x / 65535;
            buffer[offset + 2] = position.y / 65535;
            buffer[offset + 3] = POINT_PIXEL_SIZE / 255f;
            buffer[offset + 4] = _color;

            shapeCount++;
        }
        
        public void AddPoint(float2 position, float pixelSize)
        {
            _ensureCapacity(shapeCount + 1);
            
            int offset = shapeCount * GizmoUtil.FLOAT_PER_SHAPE;
            buffer[offset + 0] = 0;
            buffer[offset + 1] = position.x / 65535;
            buffer[offset + 2] = position.y / 65535;
            buffer[offset + 3] = pixelSize / 255f;
            buffer[offset + 4] = _color;

            shapeCount++;
        }

        private void _ensureCapacity(int targetCapacity)
        {
            if (buffer.IsCreated && buffer.Length > targetCapacity * GizmoUtil.FLOAT_PER_SHAPE)
            {
                return;
            }

            int newCapacity = BMath.CeilToInt(targetCapacity * GizmoUtil.FLOAT_PER_SHAPE / 128.0f) * 128;
            NativeArray<float> newBuffer = Native.PermArray<float>(newCapacity);

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