using Beast.Claw.Graphic;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Gizmo
{
    public class GizmoHandle : IGizmoHandle
    {
        private const int DEFAULT_SCREEN_POINT_SIZE = 4;
        
        public bool IsDisposed { get; private set; }

        public HalfStream halfStream;

        private ushort _colorX;
        private ushort _colorY;

        public GizmoHandle(int pixelComponentCount, int pixelPerEntity)
        {
            halfStream = HalfStream.CreateReadWrite(pixelComponentCount, pixelPerEntity, Allocator.Persistent, false);
            
            BConvert.ColorToHalf2(Color.white, out _colorX, out _colorY);
        }

        public void SetColor(Color color)
        {
            BConvert.ColorToHalf2(color, out _colorX, out _colorY);
        }

        public void Clear()
        {
            halfStream.Clear();
        }

        public void AddScreenPoint(float2 position)
        {
            AddScreenPoint(position, DEFAULT_SCREEN_POINT_SIZE);
        }
        
        // In the future we will probably create NativeBinaryStream,
        // so we will have to forget about this byte boilerplate.
        public void AddScreenPoint(float2 position, float pixelSize)
        {
            NativeSlice<ushort> buffer = halfStream.WriteNextEntity();
            
            BConvert.FloatToHalf2(position.x, out ushort px0, out ushort px1);
            BConvert.FloatToHalf2(position.y, out ushort py0, out ushort py1);
            
            buffer[0] = BConvert.FloatToHalf(0);
            buffer[1] = BConvert.FloatToHalf(pixelSize);
            buffer[2] = _colorX;
            buffer[3] = _colorY;
            buffer[4] = px0;
            buffer[5] = px1;
            buffer[6] = py0;
            buffer[7] = py1;
        }

        public void AddWorldQuad(float2 position, float worldSize)
        {
            NativeSlice<ushort> buffer = halfStream.WriteNextEntity();
            
            BConvert.FloatToHalf2(position.x, out ushort px0, out ushort px1);
            BConvert.FloatToHalf2(position.y, out ushort py0, out ushort py1);
            
            buffer[0] = BConvert.FloatToHalf(1);
            buffer[1] = BConvert.FloatToHalf(worldSize);
            buffer[2] = _colorX;
            buffer[3] = _colorY;
            buffer[4] = px0;
            buffer[5] = px1;
            buffer[6] = py0;
            buffer[7] = py1;
        }

        public void Dispose()
        {
            IsDisposed = true;
            
            halfStream.Dispose();
        }
    }
}