using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct TexturePixelPlotterForHalfStream : IPixelPlotter
    {
        public NativeSlice<ushort> output;
        public int writtenPixelCount;
        public int2 textureSize;
        public int halfPerPixel;
        
        public void AddPixel(int x, int y)
        {
            if (x < 0 || x > textureSize.x || 
                y < 0 || y > textureSize.y)
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