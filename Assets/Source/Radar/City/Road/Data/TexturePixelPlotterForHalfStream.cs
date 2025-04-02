using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct TexturePixelPlotterForHalfStream : IPixelPlotter
    {
        public NativeSlice<ushort> output;
        public int writtenPixelCount;
        public Bounds2D textureBounds;
        public int halfPerPixel;
        
        public void AddPixel(int x, int y)
        {
            if (!textureBounds.Contains(new float2(x, y)))
            {
                return;
            }
            
            int offset = writtenPixelCount * halfPerPixel;

            output[offset] = (ushort)x;
            output[offset + 1] = (ushort)y;

            writtenPixelCount++;
        }
    }
}